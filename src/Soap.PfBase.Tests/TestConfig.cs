using System.Collections.Generic;
using System.Reflection;
using DataStore;
using DataStore.Interfaces;
using Soap.Bus;
using Soap.MessagePipeline.Context;
using Soap.NotificationServer;
using Soap.NotificationServer.Channels;

/* using a config to provide values for bootstrapping tests 
keeps it more line with the production version although there are too many differences
to share any more than the general flow */

public class TestConfig : IBootstrapVariables
{
    public string ApplicationName { get; set; } = $"Domain Tests -> {Assembly.GetEntryAssembly().GetName().Name}";

    public string ApplicationVersion { get; set; } = "0.0.0";

    public IDatabaseSettings DatabaseSettings { get; set; } =
        new InMemoryDocumentRepository.Settings(new InMemoryDocumentRepository());

    public IBusSettings BusSettings { get; set; } = new InMemoryBus.Settings();

    public string DefaultExceptionMessage { get; set; } = "An Error Has Occurred";

    public AppEnvIdentifier AppEnvId { get; set; } = new AppEnvIdentifier("DOMAIN-TESTS", SoapEnvironments.InMemory);

    public NotificationServer.Settings NotificationServerSettings { get; set; } = new NotificationServer.Settings
    {
        ChannelSettings = new List<INotificationChannelSettings>
        {
            new InMemoryChannel.Settings()
        }
    };

    public bool ReturnExplicitErrorMessages { get; set; } = true;
}