using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Services
{
    public class GoogleService : IGoogleService
    {
        private readonly JsonApiDbContext _context;
        private readonly IJsonApiManualService _manualService;

        public GoogleService(JsonApiDbContext context, IJsonApiManualService jsonApiManualService)
        {
            _context = context;
            _manualService = jsonApiManualService;
        }

        public class GoogleInfos
        {
            public string GoogleAppName { get; set; }
            public string GoogleIdSite { get; set; }
            public string RedirectUriLogin { get; set; }
            public string RedirectAfterGoogleLogin { get; set; }
            public string RedirectAfterGoogleRegister { get; set; }
            public string RedirectAfterGoogleError { get; set; }
            public string GoogleAPIKey { get; set; }
            public string GoogleSecret { get; set; }
            public string GoogleAPPId { get; set; }
            public string GoogleScopes { get; set; }
            public string GoogleVerifierCode { get; set; }
        }

        public Task<GoogleInfos> GetGoogleInfos(Setup setup)
        {
            return Task.FromResult(JsonConvert.DeserializeObject<GoogleInfos>(setup.googleCredentials));
        }

        public async Task<string> GetGoogleCredentials(string code, bool forceRefreshToken = false, string userEmail = "", string redirectUri = "", string verifierCode = "")
        {
            Setup webSetup = _manualService.FirstOrDefault<Setup, int>(x => x.environment == "web");
            GoogleInfos googleInfos = await GetGoogleInfos(webSetup);

            try
            {
                List<KeyValuePair<string, string>> tokenRequestParams = [
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", googleInfos.GoogleIdSite),
                    new KeyValuePair<string, string>("client_secret", googleInfos.GoogleSecret),
                    new KeyValuePair<string, string>("redirect_uri", string.IsNullOrEmpty(redirectUri) ? googleInfos.RedirectUriLogin : redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code_verifier", string.IsNullOrEmpty(verifierCode) ? googleInfos.GoogleVerifierCode : verifierCode)
                ];

                if (forceRefreshToken)
                {
                    tokenRequestParams.Add(new KeyValuePair<string, string>("access_type", "offline"));
                    tokenRequestParams.Add(new KeyValuePair<string, string>("prompt", "consent"));
                }

                FormUrlEncodedContent contentBody = new(tokenRequestParams);

                using HttpClient httpClient = new();
                HttpResponseMessage response = await httpClient.PostAsync("https://accounts.google.com/o/oauth2/token", contentBody);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    GoogleAuthModel responseToken = JsonConvert.DeserializeObject<GoogleAuthModel>(responseData);

                    HttpResponseMessage getResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + responseToken.id_token);
                    string getResponseString = await getResponse.Content.ReadAsStringAsync();

                    GoogleProfileModel userInfo = JsonConvert.DeserializeObject<GoogleProfileModel>(getResponseString);

                    User utente = await _manualService.GetUserByEmail(userInfo.email);
                    ThirdPartsToken existingRow = await _manualService.GetExistingThirdPartAssociation(userInfo.email);

                    if (utente == null)
                    {
                        if (!webSetup.publicRegistration)
                        {
                            return googleInfos.RedirectAfterGoogleError + "?error=NRA";
                        }

                        Otp oldOtp = _manualService.FirstOrDefault<Otp, string>(x => x.UserId == "registering" && x.OtpValue == userInfo.email && x.IsValid);

                        if (oldOtp != null)
                        {
                            oldOtp.IsValid = false;
                            await _manualService.UpdateAsync<Otp, string>(oldOtp);
                        }

                        Otp otp = await _manualService.GenerateNewOtp("registering", userInfo.email, 0);

                        if (existingRow == null) // First time
                        {
                            ThirdPartsToken newRow = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserId = "ToBeRegistered",
                                Email = userInfo.email,
                                AccessToken = responseToken.access_token,
                                RefreshToken = responseToken.refresh_token,
                                AccessType = "google",
                                OtpId = otp.Id
                            };

                            await _manualService.CreateAsync<ThirdPartsToken, string>(newRow);

                            return googleInfos.RedirectAfterGoogleRegister + "?otp=" + otp.OtpSended;
                        }
                        else // Subsequent
                        {
                            existingRow.AccessToken = responseToken.access_token;

                            if (responseToken.refresh_token != null)
                            {
                                existingRow.RefreshToken = responseToken.refresh_token;
                            }

                            existingRow.OtpId = otp.Id;

                            await _manualService.UpdateAsync<ThirdPartsToken, string>(existingRow);

                            return googleInfos.RedirectAfterGoogleRegister + "?otp=" + otp.OtpSended;
                        }
                    }
                    else if (!string.IsNullOrEmpty(userEmail) && userEmail != utente.Email)
                    {
                        return "wrongEmail";
                    }
                    else // Login
                    {
                        Otp oldOtp = _manualService.FirstOrDefault<Otp, string>(x => x.UserId == utente.Id && x.OtpValue == userInfo.email && x.IsValid);

                        if (oldOtp != null)
                        {
                            oldOtp.IsValid = false;
                            await _manualService.UpdateAsync<Otp, string>(oldOtp);
                        }

                        Otp otp = await _manualService.GenerateNewOtp(utente.Id, userInfo.email, utente.TenantId);

                        if (existingRow == null) // First time
                        {
                            ThirdPartsToken newRow = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserId = utente.Id,
                                Email = userInfo.email,
                                AccessToken = responseToken.access_token,
                                RefreshToken = responseToken.refresh_token,
                                AccessType = "google",
                                OtpId = otp.Id
                            };

                            await _manualService.CreateAsync<ThirdPartsToken, string>(newRow);

                            return googleInfos.RedirectAfterGoogleLogin + "?otp=" + otp.OtpSended;
                        }
                        else // Subsequent
                        {
                            existingRow.AccessToken = responseToken.access_token;
                            if (responseToken.refresh_token != null)
                            {
                                existingRow.RefreshToken = responseToken.refresh_token;
                            }
                            existingRow.OtpId = otp.Id;

                            await _manualService.UpdateAsync<ThirdPartsToken, string>(existingRow);

                            return googleInfos.RedirectAfterGoogleLogin + "?otp=" + otp.OtpSended;
                        }
                    }
                }

                return googleInfos.RedirectAfterGoogleError;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return googleInfos.RedirectAfterGoogleError;
            }
        }

        public async Task<string> GetAccessTokenByRefreshToken(string refreshToken)
        {
            Setup webSetup = _manualService.FirstOrDefault<Setup, int>(x => x.environment == "web");
            GoogleInfos googleInfos = await GetGoogleInfos(webSetup);
            try
            {
                FormUrlEncodedContent contentBody = new(
                [
                    new KeyValuePair<string, string>("client_id", googleInfos.GoogleIdSite),
                    new KeyValuePair<string, string>("client_secret", googleInfos.GoogleSecret),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                ]);
                using HttpClient httpClient = new();
                HttpResponseMessage response = await httpClient.PostAsync("https://accounts.google.com/o/oauth2/token", contentBody);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    GoogleAuthModel responseToken = JsonConvert.DeserializeObject<GoogleAuthModel>(responseData);
                    return responseToken.access_token;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        public async Task<string> TryAddGoogleEvent(ThirdPartsToken tokens, string calendarName, Event newEvent, bool useRefreshToken = true)
        {
            try
            {
                GoogleInfos googleInfos = await GetGoogleInfos(_manualService.FirstOrDefault<Setup, int>(x => x.environment == "web"));

                // Create the Calendar service using the provided access token
                GoogleCredential credential = GoogleCredential.FromAccessToken(tokens.AccessToken)
                    .CreateScoped(CalendarService.Scope.Calendar);

                CalendarService service = new(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = googleInfos.GoogleAppName,
                });

                string calendarId = CreateCalendarIfNotExists(service, calendarName);

                if (calendarId == null && useRefreshToken)
                {
                    tokens.AccessToken = await GetAccessTokenByRefreshToken(tokens.RefreshToken);

                    if (string.IsNullOrEmpty(tokens.AccessToken))
                    {
                        return string.Empty;
                    }

                    _context.Update(tokens);
                    _context.SaveChanges();

                    return await TryAddGoogleEvent(tokens, calendarName, newEvent, false);
                }
                else if (string.IsNullOrEmpty(calendarId))
                {
                    return string.Empty;
                }
                else
                {
                    // Insert the event into the primary calendar
                    EventsResource.InsertRequest request = service.Events.Insert(newEvent, calendarId);
                    Event createdEvent = await request.ExecuteAsync();

                    return createdEvent.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<bool> TryRemoveGoogleEvent(ThirdPartsToken tokens, string eventId, string calendarName, bool useRefreshToken = true)
        {
            try
            {
                GoogleInfos googleInfos = await GetGoogleInfos(_manualService.FirstOrDefault<Setup, int>(x => x.environment == "web"));
                GoogleCredential credential = GoogleCredential.FromAccessToken(tokens.AccessToken)
                    .CreateScoped(CalendarService.Scope.Calendar);
                CalendarService service = new(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = googleInfos.GoogleAppName,
                });
                string calendarId = CreateCalendarIfNotExists(service, calendarName);

                if (calendarId == null && useRefreshToken)
                {
                    tokens.AccessToken = await GetAccessTokenByRefreshToken(tokens.RefreshToken);

                    if (string.IsNullOrEmpty(tokens.AccessToken))
                    {
                        return false;
                    }

                    _context.Update(tokens);
                    _context.SaveChanges();

                    return await TryRemoveGoogleEvent(tokens, eventId, calendarName, false);
                }
                else if (string.IsNullOrEmpty(calendarId))
                {
                    return false;
                }
                EventsResource.DeleteRequest request = service.Events.Delete(calendarId, eventId);
                await request.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public string CreateCalendarIfNotExists(CalendarService calendarService, string calendarName)
        {
            try
            {
                // Check if the calendar already exists
                CalendarListResource.ListRequest calendarListRequest = calendarService.CalendarList.List();
                CalendarList calendarListResponse = calendarListRequest.Execute();
                CalendarListEntry calendarExists = calendarListResponse.Items.FirstOrDefault(c => c.Summary == calendarName);

                if (calendarExists != null)
                {
                    return calendarExists.Id;
                }

                // Create a new calendar
                Calendar newCalendar = new()
                {
                    Summary = calendarName,
                    TimeZone = "UTC"
                };
                CalendarsResource.InsertRequest insertRequest = calendarService.Calendars.Insert(newCalendar);
                Calendar calendarCreated = insertRequest.Execute();
                return calendarCreated.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }

}
