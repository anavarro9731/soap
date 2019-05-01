namespace Soap.Integrations.MailGun
{
    using System;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Mailer.NET.Mailer;
    using Mailer.NET.Mailer.Response;
    using Mailer.NET.Mailer.Transport;
    using Soap.If.Utility.PureFunctions;

    public class EmailSender
    {
        private readonly MailGunEmailSenderSettings emailSettings;

        private readonly MailgunTransport mailgunClient;

        private readonly IMessageAggregator messageAggregator;

        public EmailSender(MailGunEmailSenderSettings settings, IMessageAggregator messageAggregator)
        {
            this.mailgunClient = new MailgunTransport(settings.MailGunDomain, settings.ApiKey);
            this.emailSettings = settings;
            this.messageAggregator = messageAggregator;
        }

        public EmailResponse SendEmail(string text, string subject, string[] sendTo)
        {
            foreach (var s in sendTo)
                Guard.Against(
                    () => !this.emailSettings.AllowedTo.Contains("*") && !this.emailSettings.AllowedTo.Contains(s),
                    $"Sending emails to {s} is not allowed.");

            return this.messageAggregator.CollectAndForward(new SendingEmail(text, subject, sendTo)).To(SendViaMailGun);
        }

        private EmailResponse SendViaMailGun(SendingEmail sendingEmail)
        {
            try
            {
                var emailMessage = new Email(this.mailgunClient)
                {
                    Subject = sendingEmail.Subject,
                    Message = sendingEmail.Text,
                    From = new Contact
                    {
                        Email = this.emailSettings.From
                    },
                    To = sendingEmail.SendTo.Select(
                                         x => new Contact
                                         {
                                             Email = x
                                         })
                                     .ToList()
                };
                var result = this.mailgunClient.SendEmail(emailMessage);
                return result;
            }
            catch (Exception e)
            {
                return new EmailResponse
                {
                    Message = e.Message, Success = false
                };
            }
        }

        public class SendingEmail : IChangeState
        {
            public SendingEmail(string text, string subject, string[] sendTo)
            {
                Text = text;
                Subject = subject;
                SendTo = sendTo;
            }

            public string[] SendTo { get; }

            public double StateOperationCost { get; set; }

            public TimeSpan? StateOperationDuration { get; set; }

            public long StateOperationStartTimestamp { get; set; }

            public long? StateOperationStopTimestamp { get; set; }

            public string Subject { get; }

            public string Text { get; }
        }
    }
}