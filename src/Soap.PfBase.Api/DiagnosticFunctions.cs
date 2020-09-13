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
        public static string GetSchema(ApplicationConfig appConfig, Assembly messagesAssembly)
        {
            IEnumerable<ApiMessage> messages = messagesAssembly.GetTypes()
                                                               .Where(t => t.InheritsOrImplements(typeof(ApiMessage)))
                                                               .Select(x => Activator.CreateInstance(x) as ApiMessage)
                                                               .ToList();

            var schema = CachedSchema.Create(appConfig, messages).Value.Schema;

            return schema;
        }

        public static async Task OnOutputStreamReadyToBeWrittenTo<TPing, TPong, TIdentity>(
            Stream outputStream,
            HttpContent httpContent,
            TransportContext transportContext,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions)
            where TPing : ApiCommand, new() where TIdentity : class, IApiIdentity, new() where TPong : ApiEvent
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

                await WriteLine(GetConfigDetails(appConfig));

                await CheckServiceBusConfiguration(appConfig, messagesAssembly, mapMessagesToFunctions, logger, WriteLine);

                await GetMessageTestResults<TPing, TPong, TIdentity>(logger, appConfig, mapMessagesToFunctions, WriteLine);
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

        private static async Task CheckServiceBusConfiguration(
            ApplicationConfig applicationConfig,
            Assembly messagesAssembly,
            MapMessagesToFunctions mapMessagesToFunctions,
            ILogger logger,
            Func<string, ValueTask> writeLine)
        {
            {
                await writeLine("Checking Service Bus Configuration...");

                var busSettings = applicationConfig.BusSettings as AzureBus.Settings;
                Guard.Against(busSettings == null, "Expected type of AzureBus");

                var azure = await Authenticate();

                var serviceBusNamespace = await ServiceBusManagementFunctions.GetNamespace(busSettings, azure, writeLine);
                await ServiceBusManagementFunctions.CreateTopicsIfNotExist(serviceBusNamespace, messagesAssembly, writeLine);

                await ServiceBusManagementFunctions.CreateSubscriptionsIfNotExist(
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

            static async Task<IAzure> Authenticate()
            {
                var azure = await Azure.Configure()
                                       .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                                       .Authenticate(GetCredentials())
                                       .WithDefaultSubscriptionAsync();
                return azure;
            }
        }

        private static string GetConfigDetails(ApplicationConfig appConfig)
        {
            var ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
                               .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                               .ToString();

            var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);

            var configAsObject = new
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

            var json = JsonConvert.SerializeObject(configAsObject, Formatting.Indented);

            return json;
        }

        private static async Task GetMessageTestResults<TPing, TPong, TIdentity>(
            ILogger logger,
            ApplicationConfig appConfig,
            MapMessagesToFunctions mappings,
            Func<string, ValueTask> writeLine)
            where TPing : ApiCommand, new() where TIdentity : class, IApiIdentity, new() where TPong : ApiEvent
        {
            await writeLine("Running Message Test...");

            var message = new TPing();
            message.Headers.EnsureRequiredHeaders();

            await writeLine($"Sending {typeof(TPing).Name} ...");

            //*  should publish e150pong which we subscribe to so it should come back to us
            var r = await AzureFunctionContext.Execute<TIdentity>(
                        message.ToNewtonsoftJson(),
                        mappings,
                        message.Headers.GetMessageId().ToString(),
                        new Dictionary<string, object>
                        {
                            { "Type", message.GetType().AssemblyQualifiedName }
                        },
                        logger,
                        appConfig);

            await writeLine(JsonConvert.SerializeObject(r, Formatting.Indented));

            if (r.Success)
            {
                var pongId = r.PublishMessages.Single().Headers.GetMessageId();
                await writeLine($"Waiting for {typeof(TPong).Name} with id {pongId}");
                var tries = 5;
                while (tries > 0)
                {
                    await writeLine("Waiting 1 seconds ...");
                    await Task.Delay(1000);
                    var logged =
                        await new DataStore(appConfig.DatabaseSettings.CreateRepository()).ReadById<MessageLogEntry>(pongId);
                    if (logged != null)
                    {
                        await writeLine($"Received {typeof(TPong).Name} Message Test Succeeded.");
                        await writeLine("+");
                        return;
                    }

                    tries--;
                }

                await writeLine($"Did not receive {typeof(TPong).Name} response!. Message Test Failure.");
                await writeLine("-");
            }
        }
    }
}