namespace Soap.Pf.MsmqEndpointBase
{
    public class MsmqEndpointConfiguration<TUserAuthenticator> where TUserAuthenticator : IAuthenticateUsers
    {
        private readonly ILogger logger;

        private readonly Startup service;

        public MsmqEndpointConfiguration(
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Action<IContainer> addBusToContainerFunc,
            Func<IDocumentRepository> documentRepositoryFactory)
        {
            try
            {
                CreateLogger(out this.logger);

                this.service = new Startup(
                    addBusToContainerFunc,
                    documentRepositoryFactory,
                    domainLogicAssembly,
                    domainMessagesAssembly);
            }
            catch (Exception ex)
            {
                //add fail-safe sources only                        
                Trace.TraceError($"Startup Error {ex}");
                this.logger?.Error(ex, "Startup Error");

                //Prevent the app continuing if the error occurs during startup
                throw new Exception("Startup Error", ex);
            }

            void CreateLogger(out ILogger logger)
            {
                EnvironmentConfig.DefineLoggingPolicyPerEnvironment(out var loggingPolicy);
                logger = loggingPolicy.CreateLogger(); // create serilog ILogger
                Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
            }
        }

        private static IEnvironmentSpecificConfig EnvironmentConfig => EndpointSetup.GetConfiguration();

        public MsmqEndpointConfiguration<TUserAuthenticator> ConfigureContainer(Action<ContainerBuilder> configureContainerAction)
        {
            this.service.ContainerActions.Add(configureContainerAction);
            return this;
        }

        public void Start(MsmqEndpointWindowsServiceSettings serviceSettings, IApiCommand startupCommand = null)
        {
            {
                try
                {
                    HostFactory.Run(
                        x =>
                            {
                            x.UseSerilog(Log.Logger);

                            x.OnException(
                                exception =>
                                    {
                                    this.logger.Error(
                                        exception,
                                        "Unhandled Exception in MSMQ Endpoint Startup {Message}.",
                                        exception.Message);
                                    Log.CloseAndFlush(); //make sur we get the error before the thread dies 
                                    });

                            x.Service<Startup>(
                                s =>
                                    {
                                    s.ConstructUsing(() => this.service);
                                    s.WhenStarted(tc => tc.Start(startupCommand));
                                    s.WhenStopped(tc => tc.Stop());
                                    });

                            x.RunAsLocalSystem();

                            x.SetDescription(serviceSettings.Description);
                            x.SetDisplayName(serviceSettings.DisplayName);
                            x.SetServiceName(serviceSettings.Name);

                            if (serviceSettings.StartAutomatically)
                            {
                                x.StartAutomatically();
                            }
                            else
                            {
                                x.StartManually();
                            }
                            });
                }
                catch (Exception ex)
                {
                    //add fail-safe sources only                        
                    Trace.TraceError($"Startup Error {ex}");

                    this.logger.Error(ex, "Startup Error {Message}", ex.Message);
                    Log.CloseAndFlush(); //make sure we get the error before thread dies

                    //Prevent the app continuing if the error occurs during startup
                    throw new Exception("Startup Error", ex);
                }
            }
        }

        public class Startup
        {
            public readonly List<Action<ContainerBuilder>> ContainerActions = new List<Action<ContainerBuilder>>();

            private readonly Action<IContainer> addBusToContainerFunc;

            private readonly Func<IDocumentRepository> documentRepositoryFactory;

            private readonly Assembly domainLogicAssembly;

            private readonly Assembly domainMessagesAssembly;

            private readonly IEnumerable<Assembly> handlerAssemblies = new[]
            {
                Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly()
            };

            private IContainer container;

            public Startup(
                Action<IContainer> addBusToContainerFunc,
                Func<IDocumentRepository> documentRepositoryFactory,
                Assembly domainLogicAssembly,
                Assembly domainMessagesAssembly)
            {
                this.addBusToContainerFunc = addBusToContainerFunc;
                this.documentRepositoryFactory = documentRepositoryFactory;
                this.domainLogicAssembly = domainLogicAssembly;
                this.domainMessagesAssembly = domainMessagesAssembly;
            }

            public static void AddHandlers(ContainerBuilder builder, IEnumerable<Assembly> handlerAssemblies)
            {
                foreach (var handlerAssembly in handlerAssemblies)
                {
                    builder.RegisterAssemblyTypes(handlerAssembly)
                           .As<IMessageHandler>()
                           .AsClosedTypesOf(typeof(CommandHandler<>))
                           .OnActivated(
                               e =>
                                   {
                                   (e.Instance as MessageHandlerBase).SetDependencies(
                                       e.Context.Resolve<IDataStore>(),
                                       e.Context.Resolve<UnitOfWork>(),
                                       e.Context.Resolve<ILogger>(),
                                       e.Context.Resolve<IMessageAggregator>());
                                   })
                           .InstancePerLifetimeScope();

                    builder.RegisterAssemblyTypes(handlerAssembly)
                           .As<IMessageHandler>()
                           .AsClosedTypesOf(typeof(CommandHandler<,>))
                           .OnActivated(
                               e =>
                                   {
                                   (e.Instance as MessageHandlerBase).SetDependencies(
                                       e.Context.Resolve<IDataStore>(),
                                       e.Context.Resolve<UnitOfWork>(),
                                       e.Context.Resolve<ILogger>(),
                                       e.Context.Resolve<IMessageAggregator>());
                                   })
                           .InstancePerLifetimeScope();
                }
            }

            public void Start(IApiCommand startupCommand)
            {
                {
                    var builder = new ContainerBuilder();

                    EndpointSetup.ConfigureCore<TUserAuthenticator>(
                        builder,
                        EnvironmentConfig.Variables,
                        new[]
                        {
                            this.domainLogicAssembly, SoapPfDomainLogicBase.GetAssembly
                        }.ToList(),
                        new[]
                        {
                            this.domainMessagesAssembly
                        }.ToList(),
                        MessageAggregator.Create,
                        this.ContainerActions);

                    AddHandlers(builder, this.handlerAssemblies); //push down?

                    builder.RegisterType<RebusCommandHandler>().AsSelf().AsImplementedInterfaces().InstancePerDependency();

                    this.container = builder.Build();

                    this.addBusToContainerFunc(this.container);

                    EndpointSetup.ValidateContainer(this.container);

                    //event sub and reg

                    Log.Logger.Information("Ready to receive messages");

                    if (startupCommand != null) SendStartupMessage();
                }

                void SendStartupMessage() => this.container.Resolve<IBusContext>().SendLocal(startupCommand);
            }

            public void Stop()
            {
                this.container.Dispose();

                Log.Logger.Information("No longer receiving messages");
                Log.CloseAndFlush();
            }
        }
    }
}