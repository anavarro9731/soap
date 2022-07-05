using System.Collections.Generic;
using System.Reflection;
using DataStore;
using DataStore.Interfaces;
using Soap.Bus;
using Soap.NotificationServer;
using Soap.NotificationServer.Channels;

/* using a config to provide values for bootstrapping tests 
keeps it more line with the production version although there are too many differences
to share any more than the general flow */

namespace Soap.PfBase.Tests
{
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.NotificationServer.Channels.Email;
    using NotificationServer = Soap.NotificationServer.NotificationServer;

    public class TestConfig : IApplicationConfig
    {
        public SoapEnvironments Environment { get; set; } = SoapEnvironments.InMemory;

        public string AppFriendlyName { get; set; } = $"Domain Tests -> {Assembly.GetEntryAssembly().GetName().Name}";

        public string AppId { get; set; } = "tests";

        public string ApplicationVersion { get; set; } = "0.0.0";

        public AuthLevel AuthLevel { get; set; } = AuthLevel.CheckNothing;

        public IDatabaseSettings DatabaseSettings { get; set; } =
            new InMemoryDocumentRepository.Settings(new InMemoryDocumentRepository());

        public IBusSettings BusSettings { get; set; } = new InMemoryBus.Settings();

        public string DefaultExceptionMessage { get; set; } = "An Error Has Occurred";

        public string EncryptionKey { get; set; } = "h4Yz4gYQWDDa8zwFHXK3vB6aK9yq8a6u";

        public bool UseServiceLevelAuthorityInTheAbsenceOfASecurityContext { get; set; }
        
        public NotificationServer.Settings NotificationServerSettings { get; set; } = new NotificationServer.Settings()
        {
            ChannelSettings = new []
            {
                new EmailChannel.MailJetEmailSenderSettings("apiKey", "apiSecret", "no-reply@test-company.com", "italerts@test-company.com")
            }
        };

        public string FunctionAppHostUrlWithTrailingSlash { get; set; } = "FunctionAppHostUrlWithTrailingSlash";

        public string FunctionAppHostName { get; set; } = "FunctionAppHostName";

        public string CorsOrigin { get; set; } = "CorsOrigin";
        
    }
}
