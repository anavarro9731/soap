namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using DataStore.Models.PureFunctions;
    using global::Sample.Logic;
    using global::Sample.Messages.Commands;
    using global::Sample.Models.Aggregates;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Pf.HttpEndpointBase.Controllers;
    using Soap.PfBase.Api;
    using Soap.Utility.Functions.Extensions;

    public static class CheckHealth
    {
        [FunctionName("CheckHealth")]
        public static HttpResponseMessage RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            try
            {
                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = GetContent(typeof(C100Ping).Assembly, new MappingRegistration())
                };

                return result;
            }
            catch (Exception e)
            {
                log.Log(LogLevel.Critical, e.ToString());

                var result = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(e.ToString())
                };

                return result;
            }
        }

        private static PushStreamContent GetContent(Assembly messagesAssembly, MapMessagesToFunctions messageMapper)
        {
            return new PushStreamContent(
                async (outputSteam, httpContent, transportContext) => await OnOutputStreamReadyToBeWrittenTo(
                                                                          outputSteam,
                                                                          httpContent,
                                                                          transportContext,
                                                                          messagesAssembly,
                                                                          messageMapper),
                new MediaTypeHeaderValue("text/plain"));
        }

        private static async Task<string> MessageTestResults(Serilog.ILogger logger, ApplicationConfig appConfig)
        {
            dynamic messageResult = new
            {
                Config = DiagnosticFunctions.GetConfig(appConfig),
                MessageTest = await DiagnosticFunctions.ExecuteMessageTest<C100Ping, User>(
                                  new C100Ping(),
                                  appConfig,
                                  new MappingRegistration(),
                                  logger)
            };

            var json = JsonConvert.SerializeObject(messageResult, Formatting.Indented);

            return json;
        }

        private static async Task OnOutputStreamReadyToBeWrittenTo(
            Stream outputStream,
            HttpContent httpContent,
            TransportContext transportContext,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions)
        {
            try
            {
                var output = new StringBuilder();

                AzureFunctionContext.LoadAppConfig(out var logger, out var appConfig);

                await ServiceBusInitialisation(appConfig, messagesAssembly, mapMessagesToFunctions, logger, outputStream);

                output.Append(await MessageTestResults(logger, appConfig));

                await outputStream.WriteAsync(Encoding.UTF8.GetBytes(output.ToString()));
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

        private static async Task ServiceBusInitialisation(
            ApplicationConfig applicationConfig,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions,
            Serilog.ILogger logger,
            Stream outputStream)
        {
            {
                var busSettings = applicationConfig.BusSettings as AzureBus.Settings;
                Guard.Against(busSettings == null, "Expected type of AzureBus");

                var azure = await Authenticate(); 
                
                async ValueTask WriteLine(string s)
                {
                    await outputStream.WriteAsync(Encoding.UTF8.GetBytes($"{s}{Environment.NewLine}"));
                    await outputStream.FlushAsync();
                }

                var serviceBusNamespace = await GetNamespace(busSettings, azure, WriteLine);
                await CreateTopicsIfNotExist(serviceBusNamespace, messagesAssembly, WriteLine);

                await CreateSubscriptionsIfNotExist(
                    serviceBusNamespace,
                    mapMessagesToFunctions,
                    logger,
                    messagesAssembly,
                    WriteLine);
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
                stream($"Found {events.Count()} Topics ...");

                var topics = (await serviceBusNamespace.Topics.ListAsync()).ToList();

                foreach (var @event in events)
                {
                    var topicName = @event.GetType().FullName;

                    if (topics.All(t => !String.Equals(t.Name, topicName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        stream($"Creating Topic {topicName}");
                        await serviceBusNamespace.Topics.Define(topicName)
                                           .WithDuplicateMessageDetection(TimeSpan.FromMinutes(30))
                                           .CreateAsync();
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
                stream($"Attaching to namespace {busSettings.BusNamespace}");

                var serviceBusNamespace = await azure.ServiceBusNamespaces.GetByResourceGroupAsync(
                    busSettings.ResourceGroup,
                    busSettings.BusNamespace);

                return serviceBusNamespace;
            }

            static async Task CreateSubscriptionsIfNotExist(
                IServiceBusNamespace serviceBusNamespace,
                MapMessagesToFunctions mapMessagesToFunctions,
                Serilog.ILogger logger1,
                Assembly messagesAssembly,
                Func<string, ValueTask> stream)
            {
                var eventNames = mapMessagesToFunctions.Events.Select(e => e.FullName);

                stream($"Creating Subscriptions for {eventNames.Count()} topics...");

                foreach (var eventName in eventNames)
                    
                    if ((await serviceBusNamespace.Topics.ListAsync()).Any(x => String.Equals(x.Name, eventName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var queueName = messagesAssembly.GetName().Name;

                        var topic = await serviceBusNamespace.Topics.GetByNameAsync(eventName);
                        if ((await topic.Subscriptions.ListAsync()).All(x => x.Name != queueName))
                        {
                            stream($"Creating Subscription {queueName} for topic {topic.Name}");
                            await topic.Subscriptions.Define(queueName).CreateAsync();
                        }

                        //*TODO set autoforward on subscription to bus queue somehow
                    }
                    else
                    {
                        var messageTemplate = $"Cannot subscribe to topic {eventName} which does not appear to exist";
                        logger1.Error(messageTemplate);
                        stream(messageTemplate);
                    }
            }

            static void CreateQueueIfNotExists(
                Assembly messagesAssembly,
                IServiceBusNamespace serviceBusNamespace,
                Action<string> stream)
            {
                var queueName = messagesAssembly.FullName;
                if (serviceBusNamespace.Queues.List().All(x => x.Name != queueName))
                {
                    stream($"Creating Queue {queueName}");
                    serviceBusNamespace.Queues.Define(queueName)
                                       .WithDuplicateMessageDetection(TimeSpan.FromMinutes(30))
                                       .WithMessageMovedToDeadLetterQueueOnMaxDeliveryCount(1)
                                       .Create();
                }
                else
                {
                    stream($"Queue {queueName} already exists");
                }
            }
        }
    }
}