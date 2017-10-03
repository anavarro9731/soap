namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Mailer.NET.Mailer.Response;
    using Palmtree.Api.Sso.Domain.Logic;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.ThirdPartyClients.Mailgun;

    public class SendEmailHandler : MessageHandler<SendEmail, List<EmailResponse>>
    {
        private readonly ApplicationConfiguration applicationConfiguration;

        public SendEmailHandler(ApplicationConfiguration applicationConfiguration)
        {
            this.applicationConfiguration = applicationConfiguration;
        }

        protected override Task<List<EmailResponse>> Handle(SendEmail message, ApiMessageMeta meta)
        {
            var response = new List<EmailResponse>();

            var emailSender = new EmailSender(this.applicationConfiguration.MailgunEmailSenderSettings, MessageAggregator);

            message.Message.To.ForEach(contact => response.Add(emailSender.SendEmail(message.Message.Message, message.Message.Subject, contact.Email)));

            return Task.FromResult(response);
        }
    }
}
