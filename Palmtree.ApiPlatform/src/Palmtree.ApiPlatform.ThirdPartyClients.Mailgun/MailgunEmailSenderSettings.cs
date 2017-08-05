namespace Palmtree.ApiPlatform.ThirdPartyClients.Mailgun
{

    using System.Collections.Generic;
    using DataStore.Models.PureFunctions;

    public class MailgunEmailSenderSettings
    {
        private MailgunEmailSenderSettings(string from, string apiKey, string mailgunDomain, IReadOnlyList<string> allowedTo)
        {
            From = from;
            ApiKey = apiKey;
            MailgunDomain = mailgunDomain;
            AllowedTo = allowedTo;
        }

        //the purpose of this is to prevent sending to bogus addresses when testing which causes problems for our mailgun account
        public IReadOnlyList<string> AllowedTo { get; }

        public string ApiKey { get; }

        public string From { get; }

        public string MailgunDomain { get; }

        public static MailgunEmailSenderSettings Create(string from, string apiKey, string mailgunDomain, IReadOnlyList<string> allowedTo = null)
        {
            Guard.Against(string.IsNullOrEmpty(from), $"{nameof(MailgunEmailSenderSettings)}.{nameof(from)} cannot be null");
            Guard.Against(string.IsNullOrEmpty(apiKey), $"{nameof(MailgunEmailSenderSettings)}.{nameof(apiKey)} cannot be null");
            Guard.Against(string.IsNullOrEmpty(mailgunDomain), $"{nameof(MailgunEmailSenderSettings)}.{nameof(mailgunDomain)} cannot be null");

            return new MailgunEmailSenderSettings(
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
