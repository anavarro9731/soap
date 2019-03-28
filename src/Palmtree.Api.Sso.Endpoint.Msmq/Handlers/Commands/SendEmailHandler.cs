﻿namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Linq;
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic;
    using Soap.If.MessagePipeline.Models;
    using Soap.Integrations.Mailgun;
    using Soap.Pf.MsmqEndpointBase;

    public class SendEmailHandler : CommandHandler<SendEmail>
    {
        private readonly ApplicationConfiguration applicationConfiguration;

        public SendEmailHandler(ApplicationConfiguration applicationConfiguration)
        {
            this.applicationConfiguration = applicationConfiguration;
        }

        protected override Task Handle(SendEmail message, ApiMessageMeta meta)
        {
            var emailSender = new EmailSender(this.applicationConfiguration.MailgunEmailSenderSettings, MessageAggregator);

            emailSender.SendEmail(message.Text, message.Subject, message.SendTo);

            return Task.CompletedTask;
        }
    }
}