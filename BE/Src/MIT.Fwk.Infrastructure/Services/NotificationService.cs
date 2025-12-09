using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Extensions;
using MIT.Fwk.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IJsonApiManualService _jsonService;
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;
        private readonly FirebaseApp _firebaseApp;
        private readonly WebSocketOptions _webSocketOptions;

        // Configuration constants
        private const string DefaultSanitizationPattern = "[^A-Za-z0-9 _]";
        private const int WebSocketTimeoutSeconds = 10;
        private const int FirebaseBatchSize = 500;

        public NotificationService(
            IJsonApiManualService jsonApiManualService,
            IConfiguration configuration,
            ILogService logService,
            FirebaseApp firebaseApp,
            IOptions<WebSocketOptions> webSocketOptions)
        {
            _jsonService = jsonApiManualService;
            _configuration = configuration;
            _logService = logService;
            _firebaseApp = firebaseApp;
            _webSocketOptions = webSocketOptions.Value;
        }

        #region Public Methods

        public async Task<int> SendNotification(Entities.Notification resource, Setup setup, string baseEndpoint, Dictionary<string, string> customContent = null)
        {
            int successCount = 0;
            int errorCount = 0;

            try
            {
                _logService.Info($"Starting notification send process", "NotificationService");

                // Step 1: Resolve recipients
                var users = await ResolveRecipientsAsync(resource);

                if (users.Count == 0)
                {
                    _logService.Info("No recipients found for notification", "NotificationService");
                    return 204;
                }

                _logService.Info($"Resolved {users.Count} unique recipients", "NotificationService");

                // Step 2: Preload all user devices in batch
                var userIds = users.Select(u => u.Id).ToList();
                var userDevicesMap = await GetUserDevicesBatchAsync(userIds);

                // Step 3: Preload blocks for tenant
                var blockedPushUsers = setup.pushNotifications && resource.SendPushNotification
                    ? await GetBlockedPushUsersBatchAsync(resource.TenantId)
                    : new HashSet<string>();

                var blockedEmailUsers = resource.SendEmail
                    ? await GetBlockedEmailUsersBatchAsync(resource.TenantId)
                    : new HashSet<string>();
                
                // Step 4: Prepare notification data structures
                var notificationsToAdd = new List<Entities.Notification>();
                var wsIds = new List<string>();
                var firebaseMessages = new List<Message>();

                // Step 5: Process each user
                foreach (var user in users)
                {
                    try
                    {
                        var devices = userDevicesMap.ContainsKey(user.Id)
                            ? userDevicesMap[user.Id]
                            : new List<UserDevice>();

                        // Create notification entity (if not data-only)
                        Entities.Notification createdNotification = null;
                        if (!resource.OnlyData)
                        {
                            createdNotification = new Entities.Notification
                            {
                                Id = Guid.NewGuid().ToString(),
                                Body = resource.Body,
                                Data = resource.Data,
                                DateSent = DateTime.UtcNow,
                                Email = user.Email,
                                OnlyData = resource.OnlyData,
                                Title = resource.Title,
                                UserId = user.Id,
                                Read = false,
                                PushType = resource.PushType,
                            };
                        }

                        // WebSocket preparation
                        if (setup.pushNotifications && resource.SendWebSocket)
                        {
                            var wsDevices = resource.ForceWebSocketApp
                                ? devices
                                : devices.Where(x => x.Platform != "android" && x.Platform != "ios").ToList();

                            foreach (var device in wsDevices)
                            {
                                wsIds.Add($"{user.Id}|{device.deviceHash}");
                            }
                        }

                        // Firebase Push preparation
                        if (setup.pushNotifications && resource.SendPushNotification && !blockedPushUsers.Contains(user.Id))
                        {
                            var mobileDevices = devices.Where(x => x.Platform == "android" || x.Platform == "ios").ToList();

                            foreach (var device in mobileDevices)
                            {
                                if (string.IsNullOrEmpty(device.PushToken))
                                    continue;

                                try
                                {
                                    var message = BuildFirebaseMessage(resource, device, createdNotification?.Id);
                                    if (message != null)
                                    {
                                        firebaseMessages.Add(message);
                                        if (!resource.OnlyData && createdNotification != null)
                                        {
                                            notificationsToAdd.Add(createdNotification);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logService.Error($"Failed to build Firebase message for user {user.Id}, device {device.Id}. Error: {ex.Message}", "NotificationService");
                                    errorCount++;
                                }
                            }
                        }

                        // Email sending
                        if (resource.SendEmail && !blockedEmailUsers.Contains(user.Id))
                        {
                            try
                            {
                                bool emailSent = await _jsonService.SendGenericEmail(
                                    user.UserName,
                                    resource.TemplateCode,
                                    resource.TenantId,
                                    baseEndpoint,
                                    customContent
                                );

                                if (!emailSent)
                                {
                                    _logService.Error($"Failed to send email to user {user.Id}. Template or content incorrect.", "NotificationService");
                                    errorCount++;
                                }
                                else
                                {
                                    successCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logService.Error($"Exception sending email to user {user.Id}. Error: {ex.Message}", "NotificationService");
                                errorCount++;
                            }
                        }
                        else
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Error($"Failed to process notification for user {user.Id}. Error: {ex.Message}", "NotificationService");
                        errorCount++;
                    }
                }

                // Step 6: Send Firebase messages in batch
                if (firebaseMessages.Any())
                {
                    await SendFirebaseBatchAsync(firebaseMessages);
                }

                // Step 7: Send WebSocket notifications
                if (wsIds.Any())
                {
                    await SendWebSocketNotificationAsync(resource, wsIds);
                }

                // Step 8: Save notifications to database
                if (notificationsToAdd.Any())
                {
                    foreach (var notification in notificationsToAdd.Distinct())
                    {
                        try
                        {
                            await _jsonService.CreateAsync<Entities.Notification, string>(notification);
                        }
                        catch (Exception ex)
                        {
                            _logService.Error($"Failed to save notification {notification.Id}. Error: {ex.Message}", "NotificationService");
                        }
                    }
                }

                _logService.Info($"Notification send completed. Success: {successCount}, Errors: {errorCount}", "NotificationService");

                return errorCount == 0 ? 204 : 422;
            }
            catch (Exception ex)
            {
                _logService.Error($"Critical error in SendNotification. Error: {ex.Message}", "NotificationService");
                return 500;
            }
        }

        #endregion

        #region Private Helper Methods - Recipient Resolution

        private async Task<List<User>> ResolveRecipientsAsync(Entities.Notification resource)
        {
            var userIdList = new List<string>();

            // Single user
            if (!string.IsNullOrEmpty(resource.UserId))
            {
                userIdList.Add(resource.UserId);
            }

            // Users list filter
            if (!string.IsNullOrEmpty(resource.UsersGuidList))
            {
                userIdList.AddRange(resource.UsersGuidList.Split(',', StringSplitOptions.RemoveEmptyEntries));
            }

            // Roles filter (if no users specified)
            if (userIdList.Count == 0 && !string.IsNullOrEmpty(resource.RolesGuidList))
            {
                var roles = resource.RolesGuidList.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var role in roles)
                {
                    var usersInRole = _jsonService.GetAllUsersGuidInRole(role);
                    userIdList.AddRange(usersInRole);
                }
            }

            // Tenants filter (if no users or roles specified)
            if (userIdList.Count == 0 && !string.IsNullOrEmpty(resource.TenantsIdList))
            {
                var tenants = resource.TenantsIdList.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var tenant in tenants)
                {
                    if (int.TryParse(tenant, out int tenantId))
                    {
                        var usersInTenant = await _jsonService.GetAllUsersGuidInTenant(tenantId);
                        userIdList.AddRange(usersInTenant);
                    }
                }
            }

            // Get distinct users in batch
            var distinctUserIds = userIdList.Distinct().ToList();
            return await _jsonService.GetAllUsers(distinctUserIds);
        }

        private async Task<Dictionary<string, List<UserDevice>>> GetUserDevicesBatchAsync(List<string> userIds)
        {
            var allDevices = new Dictionary<string, List<UserDevice>>();

            foreach (var userId in userIds)
            {
                var devices = await _jsonService.GetAllUserDevices(userId);
                allDevices[userId] = devices;
            }

            return allDevices;
        }

        private async Task<HashSet<string>> GetBlockedPushUsersBatchAsync(int tenantId)
        {
            // Batch query to get all users with push notifications blocked for this tenant
            // This replaces N individual CheckPushBlock() calls with a single query
            var blockedUserIds = await _jsonService.GetAllQueryable<BlockNotification, string>()
                .Where(b => b.TenantId == tenantId && b.PushBlock != null)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            return blockedUserIds.ToHashSet();
        }

        private async Task<HashSet<string>> GetBlockedEmailUsersBatchAsync(int tenantId)
        {
            // Batch query to get all users with email notifications blocked for this tenant
            // This replaces N individual CheckEmailBlock() calls with a single query
            var blockedUserIds = await _jsonService.GetAllQueryable<BlockNotification, string>()
                .Where(b => b.TenantId == tenantId && b.EmailBlock != null)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            return blockedUserIds.ToHashSet();
        }

        #endregion

        #region Private Helper Methods - Firebase

        private Message BuildFirebaseMessage(Entities.Notification resource, UserDevice device, string notificationId)
        {
            var pushData = BuildPushDataDictionary(resource);

            var message = new Message
            {
                Token = device.PushToken,
                Data = pushData.ToDictionary(k => k.Key, k => k.Value?.ToString() ?? "")
            };

            if (device.Platform == "android")
            {
                message.Android = BuildAndroidConfig(resource, pushData);

                if (!resource.OnlyData)
                {
                    message.Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = resource.Title ?? "",
                        Body = resource.Body ?? ""
                    };
                }
            }
            else if (device.Platform == "ios")
            {
                if (!resource.OnlyData)
                {
                    message.Apns = BuildIosConfig(resource);
                    message.Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = resource.Title ?? "",
                        Body = resource.Body ?? ""
                    };
                }
            }
            else
            {
                return null; // Unsupported platform
            }

            return message;
        }

        private Dictionary<string, object> BuildPushDataDictionary(Entities.Notification resource)
        {
            var data = new Dictionary<string, object>
            {
                { "pushType", resource.PushType ?? "" }
            };

            if (string.IsNullOrEmpty(resource.Data))
            {
                var json = new Dictionary<string, object>
                {
                    { "title", SanitizeText(resource.Title) },
                    { "body", SanitizeText(resource.Body) }
                };
                data.Add("pushData", JsonConvert.SerializeObject(json));
            }
            else
            {
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(resource.Data);
                jsonData["title"] = SanitizeText(resource.Title);
                jsonData["body"] = SanitizeText(resource.Body);
                data.Add("pushData", JsonConvert.SerializeObject(jsonData));
            }

            return data;
        }

        private AndroidConfig BuildAndroidConfig(Entities.Notification resource, Dictionary<string, object> data)
        {
            var tag = SanitizeText(resource.PushType);

            // Build data dictionary with additional Android-specific fields
            var androidData = data.ToDictionary(k => k.Key, k => k.Value?.ToString() ?? "");

            if (!resource.OnlyData)
            {
                androidData["notification_android_color"] = "#888888";
                androidData["notification_android_sound"] = "default";
                androidData["notification_title"] = resource.Title ?? "";
                androidData["notification_body"] = resource.Body ?? "";
            }

            var config = new AndroidConfig
            {
                TimeToLive = TimeSpan.FromDays(7),
                Priority = Priority.High,
                CollapseKey = tag,
                Data = androidData
            };

            if (!resource.OnlyData)
            {
                config.Notification = new AndroidNotification
                {
                    Title = resource.Title ?? "",
                    Body = resource.Body ?? "",
                    Color = "#888888",
                    Sound = "default",
                    ChannelId = "test",
                    Tag = tag,
                    Priority = NotificationPriority.HIGH,
                    Visibility = NotificationVisibility.PUBLIC,
                    DefaultSound = false
                };
            }

            return config;
        }

        private ApnsConfig BuildIosConfig(Entities.Notification resource)
        {
            return new ApnsConfig
            {
                Aps = new Aps
                {
                    ContentAvailable = false,
                    Alert = new ApsAlert
                    {
                        Title = resource.Title ?? "",
                        Body = resource.Body ?? "",
                        ActionLocKey = "FCM_PLUGIN_ACTIVITY"
                    },
                    CriticalSound = new CriticalSound
                    {
                        Critical = true,
                        Name = "default",
                        Volume = 1.0
                    }
                }
            };
        }

        private async Task SendFirebaseBatchAsync(List<Message> messages)
        {
            try
            {
                var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);

                // Send in batches of 500 (Firebase limit)
                for (int i = 0; i < messages.Count; i += FirebaseBatchSize)
                {
                    var batch = messages.Skip(i).Take(FirebaseBatchSize).ToList();

                    try
                    {
                        var response = await messaging.SendEachAsync(batch);

                        _logService.Info($"Firebase batch sent: {response.SuccessCount} success, {response.FailureCount} failures", "NotificationService");

                        // Log individual failures
                        for (int j = 0; j < response.Responses.Count; j++)
                        {
                            var result = response.Responses[j];
                            if (!result.IsSuccess)
                            {
                                _logService.Error($"Firebase message failed: {result.Exception?.Message}", "NotificationService");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Error($"Failed to send Firebase batch (size: {batch.Count}). Error: {ex.Message}", "NotificationService");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"Critical error in Firebase batch sending. Error: {ex.Message}", "NotificationService");
            }
        }

        #endregion

        #region Private Helper Methods - WebSocket

        private async Task SendWebSocketNotificationAsync(Entities.Notification resource, List<string> wsIds)
        {
            if (!wsIds.Any())
                return;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(WebSocketTimeoutSeconds));
                using var clientWebSocket = new ClientWebSocket();

                string wsUrl = _webSocketOptions.LaunchUrl
                    .Replace("*", "127.0.0.1")
                    .Replace("https", "wss")
                    .Replace("http", "ws");

                var serverUri = new Uri(wsUrl);

                // Connect with timeout
                await clientWebSocket.ConnectAsync(serverUri, cts.Token);
                _logService.Info("Connected to WebSocket server", "NotificationService");

                // Initial handshake
                string handshake = $"{{ \"userId\": \"backend\", \"fingerprint\": \"{Guid.NewGuid()}\" }}";
                await SendWebSocketMessageAsync(clientWebSocket, handshake, cts.Token);

                // Wait for handshake response
                var handshakeResponse = await ReceiveWebSocketMessageAsync(clientWebSocket, cts.Token);

                if (handshakeResponse.Contains("InputString not correct"))
                {
                    _logService.Error("WebSocket handshake failed", "NotificationService");
                    return;
                }

                // Prepare notification message
                dynamic wsData = new ExpandoObject();
                wsData.pushType = resource.PushType ?? "";
                wsData.title = resource.Title ?? "";
                wsData.body = resource.Body ?? "";
                wsData.messageType = "webSocket";

                if (string.IsNullOrEmpty(resource.Data))
                {
                    wsData.pushData = new
                    {
                        title = SanitizeText(resource.Title),
                        body = SanitizeText(resource.Body)
                    };
                }
                else
                {
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(resource.Data);
                    jsonData["title"] = SanitizeText(resource.Title);
                    jsonData["body"] = SanitizeText(resource.Body);
                    wsData.pushData = jsonData;
                }

                var notificationMessage = new
                {
                    ids = string.Join(",", wsIds),
                    message = wsData
                };

                // Send notification
                string messageJson = JsonConvert.SerializeObject(notificationMessage);
                await SendWebSocketMessageAsync(clientWebSocket, messageJson, cts.Token);

                // Wait for send confirmation
                var confirmationResponse = await ReceiveWebSocketMessageAsync(clientWebSocket, cts.Token);

                if (confirmationResponse.Contains("InputString not correct"))
                {
                    _logService.Error("WebSocket notification send failed", "NotificationService");
                }
                else
                {
                    _logService.Info($"WebSocket notification sent to {wsIds.Count} devices", "NotificationService");
                }

                // Close connection
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                _logService.Error($"WebSocket connection timeout after {WebSocketTimeoutSeconds} seconds", "NotificationService");
            }
            catch (Exception ex)
            {
                _logService.Error($"Failed to send WebSocket notification. Error: {ex.Message}", "NotificationService");
            }
        }

        private async Task SendWebSocketMessageAsync(ClientWebSocket webSocket, string message, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }

        private async Task<string> ReceiveWebSocketMessageAsync(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        #endregion

        #region Private Helper Methods - Utilities

        private string SanitizeText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return Regex.Replace(input, DefaultSanitizationPattern, "");
        }

        #endregion
    }
}
