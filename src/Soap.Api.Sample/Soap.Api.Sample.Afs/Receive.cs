namespace Soap.Api.Sample.Afs
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Options;
    using Destructurama;
    using global::Sample.Logic;
    using global::Sample.Models.Aggregates;
    using Microsoft.Azure.WebJobs;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.CSharp.RuntimeBinder;
    using Serilog;
    using Serilog.Exceptions;
    using Soap.Auth0;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.NotificationServer;

    public static class ReceiveMessage
    {
        [FunctionName("Receive")]
        public static async Task RunAsync(
            [ServiceBusTrigger("testqueue1", Connection = "sb-soap-dev")]
            string myQueueItem,
            string messageId) =>
            await Execute<User>(myQueueItem, new MappingRegistration(), messageId);

        private static async Task Execute<TUser>(
            string message,
            MapMessagesToFunctions mappingRegistration,
            string messageIdAsString,
            DataStoreOptions dataStoreOptions = null) where TUser : class, IApiIdentity, new()
        {
            {
                ParseMessageId(messageIdAsString, out var messageId);

                CreateMessageAggregator(out var messageAggregator);

                CreateAppConfig(out var appConfig);

                CreateLogger(appConfig.LogSettings, out var logger);

                CreateDataStore(messageAggregator, appConfig.DatabaseSettings, messageId, dataStoreOptions, out var dataStore);

                CreateNotificationServer(appConfig.NotificationSettings, out var notificationServer);

                CreateBusContext(messageAggregator, appConfig.BusSettings, out var bus);

                var context = new BoostrappedContext(
                    new Auth0Authenticator(() => new TUser()),
                    messageMapper: mappingRegistration,
                    appConfig: appConfig,
                    logger: logger,
                    bus: bus,
                    notificationServer: notificationServer,
                    dataStore: dataStore,
                    messageAggregator: messageAggregator);

                await MessagePipeline.Execute(message, message.GetType().AssemblyQualifiedName, () => context);
            }

            static void ParseMessageId(string messageIdAsString, out Guid messageId)
            {
                messageId = Guid.Parse(messageIdAsString);
            }

            static void CreateDataStore(
                IMessageAggregator messageAggregator,
                IDatabaseSettings databaseSettings,
                Guid messageId,
                DataStoreOptions dataStoreOptions,
                out DataStore dataStore)
            {
                //* override anything already there
                dataStoreOptions ??= DataStoreOptions.Create();
                dataStoreOptions.SpecifyUnitOfWorkId(messageId);

                dataStore = new DataStore(databaseSettings.CreateRepository(), messageAggregator, dataStoreOptions);
            }

            static void CreateNotificationServer(NotificationServer.Settings settings, out NotificationServer notificationServer)
            {
                notificationServer = settings.CreateServer();
            }

            static void CreateMessageAggregator(out IMessageAggregator messageAggregator)
            {
                messageAggregator = new MessageAggregatorForTesting();
            }

            void CreateBusContext(IMessageAggregator messageAggregator, IBusSettings busSettings, out IBus busContext)
            {
                busContext = busSettings.CreateBus(messageAggregator);
            }

            static void CreateLogger(SeqServerConfig seqServerConfig, out ILogger logger)
            {
                var loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", "DomainTests")
                                                                   .Enrich.WithExceptionDetails()
                                                                   .Destructure.UsingAttributes()
                                                                   .WriteTo.Seq(
                                                                       seqServerConfig.ServerUrl,
                                                                       apiKey: seqServerConfig.ApiKey);

                logger = loggerConfiguration.CreateLogger(); // create serilog ILogger
                Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
            }

            static void CreateAppConfig(out ApplicationConfig applicationConfig)
            {
                applicationConfig = new ApplicationConfig();
                return;

                //TODO read from feed and compile against our ApplicationConfig class and throw if bad
                var webClient = new WebClient();
                webClient.Headers.Add($"Authorisation={GetAuthorizationHeader()}");
                var configToCompile = webClient.DownloadString(GetFileUrl());

                static ApplicationConfig LoadAndExecute(string source)
                {
                    var compiledAssembly = Compile(source);

                    using var asm = new MemoryStream(compiledAssembly);
                    var assemblyLoadContext = new AssemblyLoadContext("ServiceConfig");

                    var assembly = assemblyLoadContext.LoadFromStream(asm);

                    var config = Activator.CreateInstance(assembly.GetTypes().Single()) as ApplicationConfig;

                    return config;

                    static byte[] Compile(string sourceCode)
                    {
                        using var peStream = new MemoryStream();
                        var result = GenerateCode(sourceCode).Emit(peStream);

                        if (!result.Success)
                        {
                            var failures = result
                                           .Diagnostics.Where(
                                               diagnostic =>
                                                   diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                                           .Select(x => x.GetMessage())
                                           .ToList();
                            var error = string.Join(' ', failures);
                            throw new Exception(error);
                        }

                        peStream.Seek(0, SeekOrigin.Begin);

                        return peStream.ToArray();

                        static CSharpCompilation GenerateCode(string sourceCode)
                        {
                            var codeString = SourceText.From(sourceCode);

                            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

                            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

                            var references = new MetadataReference[]
                            {
                                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(ApplicationConfig).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(AssemblyTargetedPatchBandAttribute).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location)
                            };

                            var compilationOptions = new CSharpCompilationOptions(
                                OutputKind.DynamicallyLinkedLibrary,
                                optimizationLevel: OptimizationLevel.Release);

                            var compilation = CSharpCompilation.Create("ServiceConfig.dll")
                                                               .WithOptions(compilationOptions)
                                                               .WithReferences(references)
                                                               .AddSyntaxTrees(parsedSyntaxTree);

                            return compilation;
                        }
                    }
                }

                static string GetFileUrl()
                {
                    var fileUrl = string.Format(
                        $"{ConfigId.AzureDevopsRepoPath}/items?path={ConfigId.ApplicationId}/{ConfigId.Environment}&`$format=json&includeContent=true&versionDescriptor.version=$branch&versionDescriptor.versionType=branch&api-version=5.1");
                    return fileUrl;
                }

                static string GetAuthorizationHeader()
                {
                    var basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{ConfigId.AzureDevopsRepoPat}"));
                    return $"Basic {basicAuth}";
                }
            }
        }
    }
}