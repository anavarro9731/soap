namespace Soap.Integrations.MailGun
{
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;
    using Soap.If.Interfaces;
    using Soap.If.Utility.PureFunctions;

    public class MailGunEmailSenderSettings : INotificationServerSettings
    {
        public MailGunEmailSenderSettings(string from, string apiKey, string mailGunDomain, IReadOnlyList<string> allowedTo)
        {
            Guard.Against(string.IsNullOrEmpty(from), $"{nameof(MailGunEmailSenderSettings)}.{nameof(from)} cannot be null");
            Guard.Against(string.IsNullOrEmpty(apiKey), $"{nameof(MailGunEmailSenderSettings)}.{nameof(apiKey)} cannot be null");
            Guard.Against(string.IsNullOrEmpty(mailGunDomain), $"{nameof(MailGunEmailSenderSettings)}.{nameof(mailGunDomain)} cannot be null");

            From = from;
            ApiKey = apiKey;
            MailGunDomain = mailGunDomain;
            AllowedTo = allowedTo ?? new[]
            {
                "*"
            };
        }

        //the purpose of this is to prevent sending to bogus addresses when testing which causes problems for our mailgun account
        public IReadOnlyList<string> AllowedTo { get; }

        public string ApiKey { get; }

        public string From { get; }

        public string MailGunDomain { get; }

        public INotifyUsers CreateServer(IMessageAggregator messageAggregator)
        {
            return new EmailSender(this, messageAggregator);
        }
    }
}