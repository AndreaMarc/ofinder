using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [AllowAnonymous]
    [Route("tickets")]
    public class TicketsCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;
        private readonly INotificationService _notificationService;

        public TicketsCustomController(
        IJsonApiManualService jsonService,
        INotificationService notificationService,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _jsonService = jsonService;
            _notificationService = notificationService;
        }

        public class HelpDeskNotificationModel
        {
            public string TicketId { get; set; }
            public string ToUserId { get; set; }
            public bool ToTicketManagement { get; set; }
        }

        [HttpPost]
        [Route("helpDeskNotifications")]
        public async Task<IActionResult> HelpDeskNotifications([FromBody] HelpDeskNotificationModel model)
        {
            /*Se toTicketManagement è true:
                inviare la notifica (solo e-mail) a tutti gli utenti del tenant 1 aventi claim isTicketManagement. Il testo sarà: "Un utente ha inviato una Richiesta di Assistenza tramite l'help-desk di MaeWay", l'oggetto sarà "Nuova richiesta di Assistenza MaeWay"
              
              Se toTicketManagement è false:
                inviare un'e-mail all'indirizzo contenuto nel record della tabella ticket avente guid "ticketId", con testo: "Il Servizio Assistenza Clienti di MaeWay ha risposto al tuo ticket. Accedi al sito o all'app per maggiori dettagli." e oggetto "Risposta dal Centro di Assistenza - MaeWay"
              
              Se toUserId è diverso da stringa vuota: inviare una notifica push all'utente avente guid toUserId, con testo "Hai ricevuto una risposta dal Servizio Assistenza Clienti di MaeWay".
              
              Per tutte le e-mail utilizzare il template "Generico", avente codice "ba753315-1902-48a5-8509-143de23451d5" (già creato in DEV).
            
              Per la notifica push, usare il valore di push-type "helpDeskResp"; non serve il "data". Il titolo è: "ASSISTENZA CLIENTI". Inviare la notifica anche tramite web-socket.*/

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            Setup setup = await _jsonService.FirstOrDefaultAsync<Setup, int>(x => x.environment == "web");

            Notification toSend = new()
            {
                TenantId = 1,
                SendEmail = true,
                TemplateCode = "ba753315-1902-48a5-8509-143de23451d5",
                SendPushNotification = false,
                SendWebSocket = false,
                ForceWebSocketApp = false,
                OnlyData = true
            };

            Dictionary<string, string> customContent = [];

            if (model.ToTicketManagement)
            {
                Ticket ticket = _jsonService.FirstOrDefault<Ticket, string>(x => x.Id == model.TicketId);
                toSend.UsersGuidList = string.Join(",", _jsonService.GetAllUsersGuidFromClaimName("isTicketManagement"));
                customContent.Add("{customObject}", $"Nuova richiesta di Assistenza - {setup.applicationName}");

                if (ticket == null)
                {
                    customContent.Add("{customText}", $"Un utente ha inviato una Richiesta di Assistenza tramite l'help-desk di {setup.applicationName}.");
                }
                else
                {
                    customContent.Add("{customText}", $"L'utente {ticket.FirstName} {ticket.LastName} ha inviato una Richiesta di Assistenza per conto di {(string.IsNullOrEmpty(ticket.Organization) ? ticket.OrganizationToBeConfirmed : ticket.Organization)} - P.I.: {ticket.Vat} tramite l'help-desk di {setup.applicationName} alle {ticket.CreationDate}.<br/><br/><pre>{ticket.Message}</pre>");
                }

                StringValues testNoLog;
                HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                    ? StatusCode(200)
                    : StatusCode(await _notificationService.SendNotification(toSend, setup, baseEndpoint, customContent));
            }
            else
            {

                Ticket ticket = _jsonService.FirstOrDefault<Ticket, string>(x => x.Id == model.TicketId);

                if (string.IsNullOrEmpty(model.ToUserId))
                {
                    customContent.Add("{customText}", $"Il Servizio Assistenza Clienti di MaeWay ha risposto al tuo ticket come segue: <br/> {ticket.Answer}");
                }
                else
                {
                    customContent.Add("{customText}", $"Il Servizio Assistenza Clienti di MaeWay ha risposto al tuo ticket. Accedi al sito o all'app per maggiori dettagli.");
                }

                customContent.Add("{customObject}", "Risposta dal Centro di Assistenza - MaeWay");

                // inviare email fuori utenti
                int emailResult = 204;

                StringValues testNoLog;
                HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                if (!(!string.IsNullOrEmpty(testNoLog) && testNoLog == "True"))
                {
                    if (!await _jsonService.SendGenericEmailToNonRegisteredEmail(ticket.Email, "ba753315-1902-48a5-8509-143de23451d5", 1, baseEndpoint, customContent))
                    {
                        emailResult = 422;
                    }
                }

                if (!string.IsNullOrEmpty(model.ToUserId))
                {
                    HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                    if (!string.IsNullOrEmpty(testNoLog) && testNoLog == "True")
                    {
                        return StatusCode(204);
                    }

                    toSend.UserId = model.ToUserId;
                    toSend.SendEmail = false;
                    toSend.SendPushNotification = true;
                    toSend.SendWebSocket = true;
                    toSend.ForceWebSocketApp = false;
                    toSend.OnlyData = false;
                    toSend.Body = "Il Servizio Assistenza Clienti di MaeWay ha risposto al tuo ticket.";
                    toSend.Title = "ASSISTENZA CLIENTI";
                    toSend.PushType = "helpDeskResp";

                    int notificationResult = await _notificationService.SendNotification(toSend, setup, baseEndpoint, customContent);

                    return StatusCode(notificationResult == 204 && emailResult == 204 ? 204 : 422);
                }

                return StatusCode(emailResult);
            }
        }
    }
}

