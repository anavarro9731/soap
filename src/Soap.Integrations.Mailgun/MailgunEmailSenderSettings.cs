namespace Soap.Integrations.MailGun
{
    using System.Collections.Generic;
    using Soap.If.Utility.PureFunctions;

    public class MailGunEmailSenderSettings
    {
        private MailGunEmailSenderSettings(string from, string apiKey, string mailGunDomain, IReadOnlyList<string> allowedTo)
        {
            From = from;
            ApiKey = apiKey;
            MailGunDomain = mailGunDomain;
            AllowedTo = allowedTo;
        }

        //the purpose of this is to prevent sending to bogus addresses when testing which causes problems for our mailgun account
        public IReadOnlyList<string> AllowedTo { get; }

        public string ApiKey { get; }

        public string From { get; }

        public string MailGunDomain { get; }

        public static MailGunEmailSenderSettings Create(string from, string apiKey, string mailgunDomain, IReadOnlyList<string> allowedTo = null)
        {
            Guard.Against(string.IsNullOrEmpty(from), $"{nameof(MailGunEmailSenderSettings)}.{nameof(from)} cannot be null");
            Guard.Against(string.IsNullOrEmpty(apiKey), $"{nameof(MailGunEmailSenderSettings)}.{nameof(apiKey)} cannot be null");
            Guard.Against(string.IsNullOrEmpty(mailgunDomain), $"{nameof(MailGunEmailSenderSettings)}.{nameof(mailgunDomain)} cannot be null");

            return new MailGunEmailSenderSettings(
                from,
                apiKey,
                mailgunDomain,
                allowedTo ?? new[]
                {
                    "*"
                });
        }
    }
}