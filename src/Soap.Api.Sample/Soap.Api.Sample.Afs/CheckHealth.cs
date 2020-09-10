namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using Newtonsoft.Json;
    using Serilog;
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
        public static async Task<HttpResponseMessage> RunAsync(
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
                log.Fatal(e.ToString());

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
            return new PushStreamContent(async 
                (outputSteam, httpContent, transportContext) => await OnOutputStreamReadyToBeWrittenTo(
                    outputSteam,
                    httpContent,
                    transportContext,
                    messagesAssembly,
                    messageMapper),
                new MediaTypeHeaderValue("text/plain"));
        }

        private static async Task<string> MessageTestResults(ILogger logger, ApplicationConfig appConfig)
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
            var output = new StringBuilder();

            AzureFunctionContext.LoadAppConfig(out var logger, out var appConfig);

            await ServiceBusInitialisation(appConfig, messagesAssembly, mapMessagesToFunctions, logger, outputStream);

            output.Append(await MessageTestResults(logger, appConfig));

            await outputStream.WriteAsync(Encoding.UTF8.GetBytes(output.ToString()));

            await outputStream.FlushAsync();

            await outputStream.DisposeAsync();
        }

        private static async Task ServiceBusInitialisation(
            ApplicationConfig applicationConfig,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions,
            ILogger logger,
            Stream outputStream)
        {
            {
                var busSettings = applicationConfig.BusSettings as AzureBus.Settings;
                Guard.Against(busSettings == null, "Expected type of AzureBus");

                var azure = await Authenticate();

                Action<string> writeLine = s => outputStream.Write(Encoding.UTF8.GetBytes($"{s}{Environment.NewLine}"));

                CreateBusNamespaceIfNotExists(busSettings, azure, out var serviceBusNamespace);

                CreateQueueIfNotExists(busSettings, serviceBusNamespace, writeLine);

                CreateTopicsIfNotExist(serviceBusNamespace, messagesAssembly, writeLine);

                await CreateSubscriptionsIfNotExist(serviceBusNamespace, mapMessagesToFunctions, logger, busSettings, writeLine);
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


            static void CreateTopicsIfNotExist(IServiceBusNamespace serviceBusNamespace, Assembly messagesAssembly, Action<string> stream)
            {
                stream("Creating Topics ...");
                IEnumerable<ApiMessage> events = messagesAssembly.GetTypes()
                                                                 .Where(t => t.InheritsOrImplements(typeof(ApiEvent)))
                                                                 .Select(x => Activator.CreateInstance(x) as ApiEvent)
                                                                 .ToList();

                var topics = serviceBusNamespace.Topics.List().ToList();

                foreach (var @event in events)
                {
                    var topicName = @event.GetType().FullName;

                    if (topics.All(t => t.Name != topicName))
                    {
                        stream($"Creating Topic {topicName}");
                        serviceBusNamespace.Topics.Define(topicName)
                                           .WithDuplicateMessageDetection(TimeSpan.FromMinutes(30))
                                           .Create();
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

            static void CreateBusNamespaceIfNotExists(
                AzureBus.Settings busSettings,
                IAzure azure,
                out IServiceBusNamespace serviceBusNamespace)
            {
                if (azure.ServiceBusNamespaces.CheckNameAvailability(busSettings.BusNamespace).IsAvailable)
                {
                    serviceBusNamespace = azure.ServiceBusNamespaces.Define(busSettings.BusNamespace)
                                               .WithRegion(Region.USWest)
                                               .WithNewResourceGroup(busSettings.ResourceGroup)
                                               .WithSku(NamespaceSku.Basic)
                                               .Create();
                }
                else
                {
                    serviceBusNamespace = azure.ServiceBusNamespaces.GetByResourceGroup(
                        busSettings.ResourceGroup,
                        busSettings.BusNamespace);
                    
                }
            }

            static async Task CreateSubscriptionsIfNotExist(
                IServiceBusNamespace serviceBusNamespace,
                MapMessagesToFunctions mapMessagesToFunctions,
                ILogger logger1,
                AzureBus.Settings busSettings,
                Action<string> stream)
            {
                stream("Creating Subscriptions ...");

                var eventNames = mapMessagesToFunctions.Events.Select(e => e.FullName);

                foreach (var eventName in eventNames)
                    if ((await serviceBusNamespace.Topics.ListAsync()).Any(x => x.Name == eventName))
                    {
                        var topic = await serviceBusNamespace.Topics.GetByNameAsync(eventName);
                        if ((await topic.Subscriptions.ListAsync()).All(x => x.Name != busSettings.QueueName))
                        {
                            stream($"Creating Subscription {busSettings.QueueName} for topic {topic.Name}");
                            await topic.Subscriptions.Define(busSettings.QueueName).CreateAsync();
                        }
                        
                        //*TODO set autoforward on subscription to bus queue somehow
                    }
                    else
                    {
                        logger1.Debug($"Cannot subscribe to topic {eventName} which does not appear to exist");
                    }
            }

            static void CreateQueueIfNotExists(
                AzureBus.Settings busSettings,
                IServiceBusNamespace serviceBusNamespace,
                Action<string> stream)
            {
                
                if (serviceBusNamespace.Queues.List().All(x => x.Name != busSettings.QueueName))
                {
                    stream($"Creating Queue {busSettings.QueueName}");
                    serviceBusNamespace.Queues.Define(busSettings.QueueName)
                                       .WithDuplicateMessageDetection(TimeSpan.FromMinutes(30))
                                       .WithMessageMovedToDeadLetterQueueOnMaxDeliveryCount(1)
                                       .Create();
                }
                else
                {
                    stream($"Queue {busSettings.QueueName} already exists");

                }
            }
        }
    }
}