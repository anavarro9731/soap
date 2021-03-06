namespace Soap.NotificationServer.Channels.Email
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Mailjet.Client;
    using Mailjet.Client.TransactionalEmails;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public class EmailChannel : IServerChannelInfo
    {
        private readonly MailJetEmailSenderSettings emailSettings;

        private readonly MailjetClient mailJetClient;

        private readonly IMessageAggregator messageAggregator;

        public string ItAlertsEmail => this.emailSettings.ItAlertsEmailAddress;

        public string GenericSenderEmail => this.emailSettings.GenericSenderAddress;
        
        public EmailChannel(MailJetEmailSenderSettings settings, IMessageAggregator messageAggregator)
        {
            this.mailJetClient = new MailjetClient(settings.ApiKey, settings.ApiSecret);
            this.emailSettings = settings;
            this.messageAggregator = messageAggregator;
        }

        public NotificationChannelTypes Type { get; } = NotificationChannelTypes.Email;

        public Task Send(Notification notification, ChannelSpecificNotificationMeta meta)
        {
            var emailMeta = meta.DirectCast<EmailNotificationSpecificNotificationMeta>(); //* should never fail due to design in NotificationServer and ChannelSpecificNotificationMeta.Type but double check
            
            var recipients = meta.Recipient.Split(';'); //* we try not to allow multiple recips but this would pass through most providers so we should handle it, we could fail the message i wouldn't say its a failure, so leave it
            
            foreach (var s in recipients)
            {
                Guard.Against(
                    !this.emailSettings.AllowedTo.Contains("*") && !this.emailSettings.AllowedTo.Contains(s),
                    $"Sending emails to {s} is not allowed.");
            }

            return this.messageAggregator.CollectAndForward(new SendingEmail(notification.Body, notification.Subject, recipients))
                       .To(Send);

            Task Send(SendingEmail sendingEmail)
            {
                var email = new TransactionalEmailBuilder().WithFrom(new SendContact(emailMeta.FromAddress ?? this.emailSettings.GenericSenderAddress))
                                                           .WithSubject(notification.Subject)
                                                           .WithHtmlPart(notification.Body)
                                                           .WithTo(new SendContact(emailMeta.Recipient))
                                                           .Build();

                return this.mailJetClient.SendTransactionalEmailAsync(email);
            }
        }

        public class MailJetEmailSenderSettings : INotificationChannelSettings
        {
            public MailJetEmailSenderSettings(
                string apiKey,
                string apiSecret,
                string genericSenderAddress,
                string itAlertsEmailAddress,
                IReadOnlyList<string> allowedTo = null)
            {
                Guard.Against(string.IsNullOrEmpty(genericSenderAddress), $"{nameof(MailJetEmailSenderSettings)}.{nameof(genericSenderAddress)} cannot be null. You must provide a backup from address, in case the user does not specify one with the Notification");
                Guard.Against(string.IsNullOrEmpty(itAlertsEmailAddress), $"{nameof(MailJetEmailSenderSettings)}.{nameof(itAlertsEmailAddress)} cannot be null. You must provide an address for IT alerts");
                Guard.Against(
                    string.IsNullOrEmpty(apiKey),
                    $"{nameof(MailJetEmailSenderSettings)}.{nameof(apiKey)} cannot be null");
                Guard.Against(
                    string.IsNullOrEmpty(apiSecret),
                    $"{nameof(MailJetEmailSenderSettings)}.{nameof(apiSecret)} cannot be null");

                GenericSenderAddress = genericSenderAddress;
                ItAlertsEmailAddress = itAlertsEmailAddress;
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

            public string GenericSenderAddress { get; }

            public string ItAlertsEmailAddress { get; }

            public IServerChannelInfo CreateChannel(IMessageAggregator messageAggregator) => new EmailChannel(this, messageAggregator);
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
