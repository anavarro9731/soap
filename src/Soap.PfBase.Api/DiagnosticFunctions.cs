namespace Soap.PfBase.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using DataStore;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Newtonsoft.Json;
    using Serilog;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class DiagnosticFunctions
    {
        private static async Task<object> ExecuteMessageTest<TMessage, TApiIdentity>(
            TMessage message,
            ApplicationConfig appConfig,
            MapMessagesToFunctions messageMapper,
            ILogger logger) where TApiIdentity : class, IApiIdentity, new() where TMessage : ApiMessage
        {
            message.Headers.EnsureRequiredHeaders();

            var r = await AzureFunctionContext.Execute<TApiIdentity>(
                        message.ToNewtonsoftJson(),
                        messageMapper,
                        message.Headers.GetMessageId().ToString(),
                        new Dictionary<string, object>
                        {
                            { "Type", message.GetType().AssemblyQualifiedName }
                        },
                        logger,
                        appConfig);
            return r;
            //Stream(r);

            // //* it should now publish e150pong which we subscribe to as well so it should come back to us and we wait for result
            // int tries = 5;
            // while ( tries > 0)
            // {
            //     await Task.Delay(1500);
            //     var logged = await new DataStore(appConfig.DatabaseSettings.CreateRepository()).ReadById<MessageLogEntry>(Guid.Empty);
            //     if (logged != null)
            //     {
            //         Stream(found result)
            //         return;
            //     }
            // }
            // stream(could not find result, failed)
        }
            
        private static object GetConfig(ApplicationConfig appConfig)
        {
            var ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
                               .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                               .ToString();

            var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);

            var healthCheck = new
            {
                HealthCheckedAt = DateTime.UtcNow,
                appConfig.ApplicationName,
                appConfig.ApplicationVersion,
                AppEnvironmentIdentifier = appConfig.AppEnvId,
                appConfig.DefaultExceptionMessage,
                EntryAssemblyVersion = version,
                Environment.MachineName,
                IpAddress = ipAddress
            };

            return healthCheck;
        }

        public static string GetSchema(ApplicationConfig appConfig, Assembly messagesAssembly)
        {
            IEnumerable<ApiMessage> messages = messagesAssembly.GetTypes()
                                                               .Where(t => t.InheritsOrImplements(typeof(ApiMessage)))
                                                               .Select(x => Activator.CreateInstance(x) as ApiMessage)
                                                               .ToList();

            var schema = CachedSchema.Create(appConfig, messages).Value.Schema;

            return schema;
        }

        public static async Task OnOutputStreamReadyToBeWrittenTo<TMsg, TIdentity>(
            Stream outputStream,
            HttpContent httpContent,
            TransportContext transportContext,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions)
            where TMsg : ApiMessage, new() where TIdentity : class, IApiIdentity, new()
        {
            async ValueTask WriteLine(string s)
            {
                await outputStream.WriteAsync(Encoding.UTF8.GetBytes($"{s}{Environment.NewLine}"));
                await outputStream.FlushAsync();
            }

            try
            {
                await WriteLine("Loading Config...");
                AzureFunctionContext.LoadAppConfig(out var logger, out var appConfig);
                await WriteLine(FormatConfig(appConfig));

                await WriteLine("Checking Service Bus Configuration...");
                await ServiceBusInitialisation(appConfig, messagesAssembly, mapMessagesToFunctions, logger, WriteLine);

                await WriteLine("Running Message Test...");
                await WriteLine(await GetMessageTestResults<TMsg, TIdentity>(logger, appConfig, mapMessagesToFunctions));
            }
            catch (Exception e)
            {
                await outputStream.WriteAsync(Encoding.UTF8.GetBytes(e.ToString()));
            }
            finally
            {
                await outputStream.FlushAsync();

                await outputStream.DisposeAsync();
            }
        }

        private static string FormatConfig(ApplicationConfig appConfig)
        {
            var config = GetConfig(appConfig);

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            return json;
        }

        private static async Task<string> GetMessageTestResults<TMsg, TIdentity>(
            ILogger logger,
            ApplicationConfig appConfig,
            MapMessagesToFunctions mappings) where TMsg : ApiMessage, new() where TIdentity : class, IApiIdentity, new()
        {
            var messageTest = await ExecuteMessageTest<TMsg, TIdentity>(new TMsg(), appConfig, mappings, logger);

            var json = JsonConvert.SerializeObject(messageTest, Formatting.Indented);

            return json;
        }

        private static async Task ServiceBusInitialisation(
            ApplicationConfig applicationConfig,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions,
            ILogger logger,
            Func<string, ValueTask> writeLine)
        {
            {
                var busSettings = applicationConfig.BusSettings as AzureBus.Settings;
                Guard.Against(busSettings == null, "Expected type of AzureBus");

                var azure = await Authenticate();

                var serviceBusNamespace = await GetNamespace(busSettings, azure, writeLine);
                await CreateTopicsIfNotExist(serviceBusNamespace, messagesAssembly, writeLine);

                await CreateSubscriptionsIfNotExist(
                    serviceBusNamespace,
                    mapMessagesToFunctions,
                    logger,
                    messagesAssembly,
                    writeLine);
            }

            static AzureCredentials GetCredentials()
            {
                var clientId = EnvVars.ServicePrincipal.ClientId;
                var clientSecret = EnvVars.ServicePrincipal.ClientSecret;
                var tenantId = EnvVars.ServicePrincipal.TenantId;

                return SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                    clientId,
                    clientSecret,
                    tenantId,
                    AzureEnvironment.AzureGlobalCloud);
            }

            static async Task CreateTopicsIfNotExist(
                IServiceBusNamespace serviceBusNamespace,
                Assembly messagesAssembly,
                Func<string, ValueTask> stream)
            {
                IEnumerable<ApiMessage> events = messagesAssembly.GetTypes()
                                                                 .Where(t => t.InheritsOrImplements(typeof(ApiEvent)))
                                                                 .Select(x => Activator.CreateInstance(x) as ApiEvent)
                                                                 .ToList();

                await stream($"Found {events.Count()} Topics published by this service ...");

                var topics = (await serviceBusNamespace.Topics.ListAsync()).ToList();

                foreach (var @event in events)
                {
                    var topicName = @event.GetType().FullName;

                    if (topics.All(t => !string.Equals(t.Name, topicName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        await stream($"-> Creating Topic {topicName}");
                        await serviceBusNamespace.Topics.Define(topicName)
                                                 .WithDuplicateMessageDetection(TimeSpan.FromMinutes(30))
                                                 .CreateAsync();
                    }
                    else
                    {
                        await stream($"-> Topic {topicName} already created");
                    }
                }
            }

            static async Task<IAzure> Authenticate()
            {
                var azure = await Azure.Configure()
                                       .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                                       .Authenticate(GetCredentials())
                                       .WithDefaultSubscriptionAsync();
                return azure;
            }

            static async Task<IServiceBusNamespace> GetNamespace(
                AzureBus.Settings busSettings,
                IAzure azure,
                Func<string, ValueTask> stream)
            {
                await stream($"Attaching to namespace {busSettings.BusNamespace}");

                var serviceBusNamespace = await azure.ServiceBusNamespaces.GetByResourceGroupAsync(
                                              busSettings.ResourceGroup,
                                              busSettings.BusNamespace);

                return serviceBusNamespace;
            }

            static async Task CreateSubscriptionsIfNotExist(
                IServiceBusNamespace serviceBusNamespace,
                MapMessagesToFunctions mapMessagesToFunctions,
                ILogger logger1,
                Assembly messagesAssembly,
                Func<string, ValueTask> stream)
            {
                var eventNames = mapMessagesToFunctions.Events.Select(e => e.FullName);

                await stream($"Found {eventNames.Count()} Topic handled by this service, checking subscriptions...");

                foreach (var eventName in eventNames)

                    if ((await serviceBusNamespace.Topics.ListAsync()).Any(
                        x => string.Equals(x.Name, eventName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var queueName = messagesAssembly.GetName().Name;

                        var topic = await serviceBusNamespace.Topics.GetByNameAsync(eventName);
                        if ((await topic.Subscriptions.ListAsync()).All(
                            x => !string.Equals(x.Name, queueName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            await stream($"-> Creating Subscription {queueName} for topic {topic.Name}");
                            await topic.Subscriptions.Define(queueName).CreateAsync();
                        }
                        else
                        {
                            await stream($"-> Subscription for topic {topic.Name} already exists");
                        }

                        //* TODO set auto-forward on subscription to bus queue somehow
                    }

                    else
                    {
                        var messageTemplate = $"-> Cannot subscribe to topic {eventName} which does not appear to exist";
                        logger1.Error(messageTemplate);
                        await stream(messageTemplate);
                    }
            }
        }
    }
}