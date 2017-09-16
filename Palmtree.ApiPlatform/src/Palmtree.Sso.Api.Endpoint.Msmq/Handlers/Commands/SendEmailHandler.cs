namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Mailer.NET.Mailer.Response;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.ThirdPartyClients.Mailgun;
    using Palmtree.Sample.Api.Domain.Logic;

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
