using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using String = System.String;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    public class TranslationCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;

        public TranslationCustomController(
        IJsonApiManualService jsonService,
        IResourceService<Translation, int> translationService,
        IResourceService<Setup, int> setupService,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _jsonService = jsonService;
        }

        public class AddVoiceModel
        {
            public string primaryRow { get; set; } //se vuoto sto inserendo una primary row
            public string label { get; set; }
            public string value { get; set; }//potrei avere come valore un {} se sto inserendo un oggetto, e in seguito potro aggiungere un valore li dentro mettendo dentro a primarow il nome di questo value
            public string type { get; set; } //indica la tipologia di voce (general,component,template...}

        }

        [HttpPost]
        [Route("translations/addVoice")]
        public async Task<IActionResult> AddVoice([FromBody] AddVoiceModel model, CancellationToken cancellationToken)
        {

            return await JsonOperationAsync(model, cancellationToken, "add");

        }

        [HttpDelete("translations/delVoice")]
        public async Task<IActionResult> DelVoice([FromBody] AddVoiceModel model, CancellationToken cancellationToken)
        {
            return await JsonOperationAsync(model, cancellationToken, "del");
        }
        [HttpPatch]
        [Route("translations/patchVoice")]
        public async Task<IActionResult> PatchVoice([FromBody] AddVoiceModel model, CancellationToken cancellationToken)
        {


            return await Patchjson(model, cancellationToken);

        }


        private String UpdateLanguageFile(JObject fileObject, AddVoiceModel model, String operation, String type = "web")
        {

            //caso type e pr vuoto -->errore
            if (model.type == "" && model.primaryRow == "")
            {



            }

            //aggiungo solo la label perche ho sia type che pr
            if (fileObject[model.type] != null && fileObject[model.type][model.primaryRow] != null)
            {

                if (operation == "add")
                {
                    //aggiungo la label solo se non esiste
                    if (fileObject[model.type][model.primaryRow][model.label] == null)
                    {
                        //var appoggio = Newtonsoft.Json.JsonConvert.DeserializeObject(fileObject[model.type][model.primaryRow] + "");
                        if (model.label == "{}")
                        {
                            JObject primaryRowObject = [];
                            // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                            fileObject[model.type][model.primaryRow] = primaryRowObject;
                        }
                        else
                        {

                            fileObject[model.type][model.primaryRow][model.label] = model.value;
                        }

                        //appoggio = operation == "add" ? appoggio.Add(model.label, model.value, model.type) : appoggio.Remove(model.label);
                        //fileObject[model.type][model.primaryRow] = appoggio;

                    }

                    //se esiste anche la label do errore
                    else
                    {


                    }
                }
                else
                {


                    if (model.label == "")
                    {
                        fileObject.Value<JObject>(model.type).Remove(model.primaryRow);
                    }
                    else
                    {
                        //rimuovo la label solo se non esiste
                        if (fileObject[model.type][model.primaryRow][model.label] != null)
                        {
                            fileObject[model.type].Value<JObject>(model.primaryRow).Remove(model.label);
                        }

                        //se non esiste la label do errore
                        else
                        {


                        }
                    }

                }



            }

            //inserisco solo la primaryrow con valore vuoto

            else if (fileObject[model.type] != null)
            {
                if (fileObject[model.type][model.primaryRow] == null)
                {
                    if (operation == "add")
                    {
                        if (model.label == "{}")
                        {
                            JObject primaryRowObject = [];
                            // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                            fileObject[model.type][model.primaryRow] = primaryRowObject;
                        }
                        else
                        {

                            fileObject[model.type][model.primaryRow] = model.label;
                        }
                    }
                    else
                    {

                    }
                    //var oldJson2 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>("{}");
                    //var appoggio = Newtonsoft.Json.JsonConvert.DeserializeObject(oldJson2 + "");
                    //fileObject = operation == "add" ? fileObject[model.type].Add(model.label, model.value, model.type) : fileObject[model.type].Remove(model.label);
                }
                else
                {
                    if (operation == "add")
                    {

                    }
                    else
                    {
                        fileObject.Value<JObject>(model.type).Remove(model.primaryRow);

                    }

                }
            }
            else if (fileObject[model.type] == null)
            {


                if (model.primaryRow == "")
                {
                    if (operation == "add")
                    {
                        JObject primaryRowObject = [];
                        // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                        fileObject[model.type] = primaryRowObject;
                    }
                    else
                    {
                        fileObject.Value<JObject>(model.type).Remove(model.primaryRow);
                    }
                }


                else if (operation == "add")
                {
                    try
                    {


                        fileObject[model.type] = new JObject();
                        // Creazione del nuovo oggetto o valore 
                        if (model.label == "{}")
                        {
                            JObject primaryRowObject = [];
                            // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                            fileObject[model.type][model.primaryRow] = primaryRowObject;
                        }
                        else
                        {

                            fileObject[model.type][model.primaryRow] = "";
                        }




                        //fileObject[model.type] = appoggioempty;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {

                }



            }




            return fileObject.ToString();
        }


        static JObject SortJObjectProperties(JObject jObject)
        {
            JObject sortedJObject = [];

            IOrderedEnumerable<JProperty> properties = jObject.Properties()
                .OrderBy(p => p.Name);

            foreach (JProperty property in properties)
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    JObject nestedObject = SortJObjectProperties((JObject)property.Value);
                    sortedJObject.Add(property.Name, nestedObject);
                }
                else
                {
                    sortedJObject.Add(property.Name, property.Value);
                }
            }

            return sortedJObject;
        }

        private async Task<IActionResult> JsonOperationAsync(AddVoiceModel model, CancellationToken cancellationToken, String operation = "add")
        {

            List<Setup> setups = await _jsonService.GetAllAsync<Setup, int>();
            Setup setupApp = setups.First(x => x.environment == "app");
            Setup setupWeb = setups.First(x => x.environment == "web");


            //aggiornamento dello schema dentro setup
            #region
            //cerco lo schema di traduzione e la trasformo in un oggetto
            JObject setupwebToUpdate = JObject.Parse(setupWeb.languageSetup);

            //se non passa type non va bene
            //se passa un type che non ho, lo creo con la primary row che mi deve passare
            //se ho quel type e non la primary, la inserisco, se ho anche la primarty inserisco il value, se non lo ho, senno errore

            //caso type e pr vuoto -->errore
            try
            {
                if (model.type == "" && model.primaryRow == "")
                {

                    JsonApiErrorModel error = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = ((int)HttpStatusCode.BadRequest).ToString(),
                        Title = "PrimaryRow e type non definite",
                        Detail = "PrimaryRow e type non definite"
                    };

                    // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                    return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };

                }

                //aggiungo solo la label perche ho sia type che pr
                if (setupwebToUpdate[model.type] != null && setupwebToUpdate[model.type][model.primaryRow] != null)
                {



                    if (operation == "add")
                    {
                        if (model.label == "" || model.label == "{}")
                        {
                            JsonApiErrorModel error = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Status = ((int)HttpStatusCode.BadRequest).ToString(),
                                Title = "primaryrow gia esistente",
                                Detail = "primaryrow gia esistente"
                            };

                            // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                            return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                        }
                        //aggiungo la label solo se non esiste
                        if (setupwebToUpdate[model.type][model.primaryRow][model.label] == null)
                        {
                            //var appoggio = Newtonsoft.Json.JsonConvert.DeserializeObject(setupwebToUpdate[model.type][model.primaryRow] + "");
                            if (model.label == "{}")
                            {
                                JObject primaryRowObject = [];
                                // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                                setupwebToUpdate[model.type][model.primaryRow] = primaryRowObject;
                            }
                            else
                            {

                                setupwebToUpdate[model.type][model.primaryRow][model.label] = model.value;
                            }

                            //appoggio = operation == "add" ? appoggio.Add(model.label, model.value, model.type) : appoggio.Remove(model.label);
                            //setupwebToUpdate[model.type][model.primaryRow] = appoggio;

                        }

                        //se esiste anche la label do errore
                        else
                        {

                            JsonApiErrorModel error = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Status = ((int)HttpStatusCode.BadRequest).ToString(),
                                Title = "label gia esistente",
                                Detail = "label gia esistente"
                            };

                            // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                            return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                        }
                    }
                    else
                    {


                        if (model.label == "")
                        {
                            JObject jObjectInterno1 = (JObject)setupwebToUpdate[model.type];
                            jObjectInterno1.Remove(model.primaryRow);
                            setupwebToUpdate[model.type] = jObjectInterno1;
                        }
                        else
                        {
                            //rimuovo la label solo se non esiste
                            if (setupwebToUpdate[model.type][model.primaryRow][model.label] != null)
                            {
                                JObject jObjectInterno1 = (JObject)setupwebToUpdate[model.type][model.primaryRow];
                                jObjectInterno1.Remove(model.label);
                                setupwebToUpdate[model.type][model.primaryRow] = jObjectInterno1;
                                //IEnumerable<dynamic> props = setupwebToUpdate.Properties();
                                //props.Where(x => x.Name.Equals(model.label)).ToList().ForEach(x => x.Remove());
                                ////setupwebToUpdate[model.type].Value<JObject>(model.primaryRow).Remove(model.label);
                                //setupwebToUpdate = JObject.Parse(props.ToJson());
                            }

                            //se non esiste la label do errore
                            else
                            {

                                JsonApiErrorModel error = new()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Status = ((int)HttpStatusCode.BadRequest).ToString(),
                                    Title = "label non esistente",
                                    Detail = "label non esistente"
                                };

                                // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                                return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                            }
                        }

                    }



                }

                //inserisco solo la primaryrow con valore vuoto

                else if (setupwebToUpdate[model.type] != null)
                {
                    if (setupwebToUpdate[model.type][model.primaryRow] == null)
                    {
                        if (operation == "add")
                        {

                            if (model.primaryRow == "" || model.primaryRow == "{}")
                            {
                                JsonApiErrorModel error = new()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Status = ((int)HttpStatusCode.BadRequest).ToString(),
                                    Title = "Type gia presente",
                                    Detail = "Type gia presente"
                                };

                                // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                                return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                            }

                            if (model.label == "{}")
                            {
                                JObject primaryRowObject = [];
                                // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                                setupwebToUpdate[model.type][model.primaryRow] = primaryRowObject;
                            }
                            else
                            {

                                setupwebToUpdate[model.type][model.primaryRow] = model.label;
                            }
                        }
                        else
                        {
                            JsonApiErrorModel error = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Status = ((int)HttpStatusCode.BadRequest).ToString(),
                                Title = "PrimaryRow non presente",
                                Detail = "PrimaryRow non presente"
                            };

                            // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                            return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                        }
                        //var oldJson2 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>("{}");
                        //var appoggio = Newtonsoft.Json.JsonConvert.DeserializeObject(oldJson2 + "");
                        //setupwebToUpdate = operation == "add" ? setupwebToUpdate[model.type].Add(model.label, model.value, model.type) : setupwebToUpdate[model.type].Remove(model.label);
                    }
                    else
                    {
                        if (operation == "add")
                        {
                            JsonApiErrorModel error = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Status = ((int)HttpStatusCode.BadRequest).ToString(),
                                Title = "PrimaryRow gia presente",
                                Detail = "PrimaryRow gia presente"
                            };

                            // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                            return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                        }
                        else
                        {
                            JObject jObjectInterno1 = (JObject)setupwebToUpdate[model.type];
                            jObjectInterno1.Remove(model.primaryRow);
                            setupwebToUpdate[model.type] = jObjectInterno1;

                        }

                    }
                }
                else if (setupwebToUpdate[model.type] == null)
                {


                    if (model.primaryRow == "")
                    {
                        if (operation == "add")
                        {
                            JObject primaryRowObject = [];
                            // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                            setupwebToUpdate[model.type] = primaryRowObject;
                        }
                        else
                        {
                            JObject jObjectInterno1 = (JObject)setupwebToUpdate[model.type];
                            jObjectInterno1.Remove(model.primaryRow);
                            setupwebToUpdate[model.type] = jObjectInterno1;
                        }
                    }


                    else if (operation == "add")
                    {
                        try
                        {


                            setupwebToUpdate[model.type] = new JObject();
                            // Creazione del nuovo oggetto o valore 
                            if (model.label == "{}")
                            {
                                JObject primaryRowObject = [];
                                // Aggiunta del nuovo oggetto annidato all'oggetto JSON esistente
                                setupwebToUpdate[model.type][model.primaryRow] = primaryRowObject;
                            }
                            else
                            {

                                setupwebToUpdate[model.type][model.primaryRow] = "";
                            }




                            //setupwebToUpdate[model.type] = appoggioempty;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        JsonApiErrorModel error = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Status = ((int)HttpStatusCode.BadRequest).ToString(),
                            Title = "impossibile cancellare type non esistente",
                            Detail = "impossibile cancellare type non esistente"
                        };

                        // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                        return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                    }



                }

                JObject sortedObj = SortJObjectProperties(setupwebToUpdate);


                setupWeb.languageSetup = sortedObj.ToString();
                setupApp.languageSetup = sortedObj.ToString();
                await _jsonService.UpdateAsync<Setup, int>(setupWeb);
                await _jsonService.UpdateAsync<Setup, int>(setupApp);







                #endregion
                // Deserializzazione della stringa JSON in una lista di oggetti
                List<dynamic> objects = JsonConvert.DeserializeObject<List<dynamic>>(setupWeb.availableLanguages);

                // Selezione dei valori di "code"
                List<string> supportedLanguages = objects.Select(o => (string)o.code).ToList();

                List<Translation> traduzioni = await _jsonService.GetAllAsync<Translation, int>();

                //aggiornamento di ciascuna lingua presente con il nuovo schema
                #region
                foreach (string lang in supportedLanguages)
                {

                    //cerco la traduzione e la trasformo in un oggetto
                    JObject fileObjectweb = JObject.Parse(traduzioni.First(x => x.languageCode == lang).translationWeb);
                    JObject fileObjectapp = JObject.Parse(traduzioni.First(x => x.languageCode == lang).translationApp);

                    string newWeb = UpdateLanguageFile(fileObjectweb, model, operation, "web");
                    string newApp = UpdateLanguageFile(fileObjectapp, model, operation, "app");

                    Translation toupdateLang = traduzioni.First(x => x.languageCode == lang);
                    toupdateLang.translationWeb = newWeb;
                    toupdateLang.translationApp = newApp;
                    await _jsonService.UpdateAsync<Translation, int>(toupdateLang);

                }
                #endregion
                List<Setup> setupAggiornato = await _jsonService.GetAllAsync<Setup, int>();

                //mando email ai traduttori che e' stata aggiunta una nuova voce da tradurre
                if (operation == "add")
                {
                    //

                }
                return StatusCode(202, setupAggiornato);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message.ToString());
            }
        }

        private async Task<IActionResult> Patchjson(AddVoiceModel model, CancellationToken cancellationToken)
        {

            List<Setup> setups = await _jsonService.GetAllAsync<Setup, int>();
            Setup setupApp = setups.First(x => x.environment == "app");
            Setup setupWeb = setups.First(x => x.environment == "web");

            JObject setupwebToUpdate = JObject.Parse(setupWeb.languageSetup);
            JToken child;
            JToken child1;
            setupwebToUpdate.TryGetValue(model.type, out child);
            (child as JObject).TryGetValue(model.primaryRow, out child1);
            string aaaa = child1.ToString();




            //mi passa tutto
            if (model.type != "" && model.primaryRow != "" && model.label != "" && model.value != "")
            {
                setupwebToUpdate[model.type][model.primaryRow][model.label] = model.value;
            }
            //mi passa type, pr,label
            else if (model.type != "" && model.primaryRow != "" && model.label != "")
            {
                setupwebToUpdate[model.type][model.primaryRow] = model.label;
            }
            //mi passa type, pr
            else if ((model.type != "" && model.primaryRow != ""))
            {
                setupwebToUpdate[model.type] = model.primaryRow;
            }
            //mi passa type
            //non va bene
            else
            {
                JsonApiErrorModel error = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Status = ((int)HttpStatusCode.BadRequest).ToString(),
                    Title = "impossibile trovare il valore da aggiornare",
                    Detail = "impossibile trovare il valore da aggiornare"
                };

                // Restituisci la risposta con lo stato appropriato e l'errore nel corpo della risposta
                return new ObjectResult(error) { StatusCode = (int)HttpStatusCode.BadRequest };
                //non ho trovato nessun percorso valido
            }

            setupWeb.languageSetup = setupwebToUpdate.ToString();
            setupApp.languageSetup = setupwebToUpdate.ToString();
            await _jsonService.UpdateAsync<Setup, int>(setupWeb);
            await _jsonService.UpdateAsync<Setup, int>(setupApp);

            List<Setup> setupAggiornato = await _jsonService.GetAllAsync<Setup, int>();
            return StatusCode(202, setupAggiornato);
        }


    }
}

