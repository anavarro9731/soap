namespace Soap.Pf.HttpEndpointBase.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.PfBase.Api;
    using Soap.Utility.Functions.Extensions;

    public static class DiagnosticFunctions
    {


        public static async Task<object> ExecuteMessageTest<TMessage, TApiIdentity>(
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
        }

        public static object GetConfig(ApplicationConfig appConfig)
        {
            var ipAddress = Dns.GetHostEntryAsync(Dns.GetHostName())
                               .Result.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                               .ToString();

            var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);

            var healthCheck = new
            {
                healthCheckedAt = DateTime.UtcNow,
                appConfig.ApplicationName,
                appConfig.ApplicationVersion,
                AppEnvironmentIdentifier = appConfig.AppEnvId,
                appConfig.DefaultExceptionMessage,
                EntryAssemblyVersion = version,
                machineName = Environment.MachineName,
                ipAddress
            };

            return healthCheck;
        }

        public static string GetSchema(ApplicationConfig appConfig, Assembly messagesAssembly)
        {
            IEnumerable<ApiMessage> messages = messagesAssembly.GetTypes()
                                                               .Where(t => t.InheritsOrImplements(typeof(ApiMessage)))
                                                               .Select(x => Activator.CreateInstance(x) as ApiMessage).ToList();
                                           

            var schema = CachedSchema.Create(appConfig, messages).Value.Schema;

            return schema;
        }
    }
}