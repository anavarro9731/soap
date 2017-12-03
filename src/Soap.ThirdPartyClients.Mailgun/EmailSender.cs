namespace Soap.Integrations.Mailgun
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Mailer.NET.Mailer;
    using Mailer.NET.Mailer.Response;
    using Mailer.NET.Mailer.Transport;
    using Soap.If.Utility.PureFunctions;

    public class EmailSender
    {
        private readonly MailgunEmailSenderSettings emailSettings;

        private readonly MailgunTransport mailgunClient;

        private readonly IMessageAggregator messageAggregator;

        public EmailSender(MailgunEmailSenderSettings settings, IMessageAggregator messageAggregator)
        {
            this.mailgunClient = new MailgunTransport(settings.MailgunDomain, settings.ApiKey);
            this.emailSettings = settings;
            this.messageAggregator = messageAggregator;
        }

        public EmailResponse SendEmail(string text, string subject, string sendTo)
        {
            Guard.Against(
                () => !this.emailSettings.AllowedTo.Contains("*") && !this.emailSettings.AllowedTo.Contains(sendTo),
                "Sending emails to this address is not allowed.");

            var emailMessage = new Email(this.mailgunClient)
            {
                Subject = subject,
                From = new Contact
                {
                    Email = this.emailSettings.From
                },
                To = new List<Contact>
                {
                    new Contact
                    {
                        Email = sendTo
                    }
                }
            };

            return this.messageAggregator.CollectAndForward(new SendingEmail(emailMessage)).To(SendViaMailGun);
        }

        private EmailResponse SendViaMailGun(SendingEmail sendingEmail)
        {
            try
            {
                var result = this.mailgunClient.SendEmail(sendingEmail.Message);
                return result;
            }
            catch (Exception e)
            {
                return new EmailResponse
                {
                    Message = e.Message,
                    Success = false
                };
            }
        }

        public class SendingEmail : IChangeState
        {
            public SendingEmail(Email message)
            {
                Message = message;
            }

            public Email Message { get; }

            public double StateOperationCost { get; set; }

            public TimeSpan? StateOperationDuration { get; set; }

            public long StateOperationStartTimestamp { get; set; }

            public long? StateOperationStopTimestamp { get; set; }
        }
    }
}