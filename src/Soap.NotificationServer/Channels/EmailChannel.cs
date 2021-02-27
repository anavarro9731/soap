namespace Soap.NotificationServer.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Mailjet.Client;
    using Mailjet.Client.TransactionalEmails;
    using Soap.Context;
    using Soap.Interfaces;

    public class EmailChannel : IServerChannelInfo
    {
        private readonly MailJetEmailSenderSettings emailSettings;

        private readonly MailjetClient mailJetClient;

        public EmailChannel(MailJetEmailSenderSettings settings)
        {
            this.mailJetClient = new MailjetClient(settings.ApiKey, settings.ApiSecret);
            this.emailSettings = settings;
        }

        public NotificationChannelTypes Type { get; } = NotificationChannelTypes.Email;

        public Task Send(Notification notification)
        {
            var recipients = notification.Recipient.Split(';');
            foreach (var s in recipients)
            {
                Guard.Against(
                    !this.emailSettings.AllowedTo.Contains("*") && !this.emailSettings.AllowedTo.Contains(s),
                    $"Sending emails to {s} is not allowed.");
            }

            return this.emailSettings.MessageAggregator
                       .CollectAndForward(new SendingEmail(notification.Body, notification.Subject, recipients))
                       .To(Send);

            Task Send(SendingEmail sendingEmail)
            {
                var email = new TransactionalEmailBuilder().WithFrom(new SendContact("ft.engineering@roseandmy.work"))
                                                           .WithSubject("Test subject")
                                                           .WithHtmlPart("<h1>Header</h1>Test Data")
                                                           .WithTo(new SendContact("ft.engineering@roseandmy.work"))
                                                           .Build();

                return this.mailJetClient.SendTransactionalEmailAsync(email);
            }
        }

        public class MailJetEmailSenderSettings : INotificationChannelSettings
        {
            internal readonly IMessageAggregator MessageAggregator;

            public MailJetEmailSenderSettings(
                string from,
                string apiKey,
                string apiSecret,
                IReadOnlyList<string> allowedTo,
                IMessageAggregator messageAggregator)
            {
                this.MessageAggregator = messageAggregator;
                Guard.Against(string.IsNullOrEmpty(from), $"{nameof(MailJetEmailSenderSettings)}.{nameof(from)} cannot be null");
                Guard.Against(
                    string.IsNullOrEmpty(apiKey),
                    $"{nameof(MailJetEmailSenderSettings)}.{nameof(apiKey)} cannot be null");
                Guard.Against(
                    string.IsNullOrEmpty(apiSecret),
                    $"{nameof(MailJetEmailSenderSettings)}.{nameof(apiSecret)} cannot be null");

                From = from;
                ApiKey = apiKey;
                ApiSecret = apiSecret;
                AllowedTo = allowedTo ?? new[]
                {
                    "*"
                };
            }

            /* the purpose of this is whilelist at config level (particularly when testing) to avoid sending to bogus addresses
             which causes problems for our reputation with providers */
            public IReadOnlyList<string> AllowedTo { get; }

            public string ApiKey { get; }

            public string ApiSecret { get; }

            public string From { get; }

            public IServerChannelInfo CreateChannel() => new EmailChannel(this);
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
