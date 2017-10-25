namespace Soap.Endpoint.Msmq.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Autofac;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;
    using Soap.MessagePipeline.Messages;
    using Topshelf;

    public class MsmqEndpointConfiguration<TUserAuthenticator> where TUserAuthenticator : IAuthenticateUsers
    {
        private readonly ILogger logger;

        private readonly Startup service;

        public MsmqEndpointConfiguration(
            Assembly domainLogicAssembly,
            Assembly domainModelsAssembly,
            Func<IContainer, IBusContext> addBusToContainerFunc,
            Func<IDocumentRepository> documentRepositoryFactory)
        {
            try
            {
                CreateLogger(out this.logger);

                this.service = new Startup(addBusToContainerFunc, documentRepositoryFactory, domainLogicAssembly, domainModelsAssembly);
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

                                x.OnException(exception => { this.logger.Error("Unhandled exception", exception); });

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
                    this.logger.Error(ex, "Startup Error");

                    //Prevent the app continuing if the error occurs during startup
                    throw new Exception("Startup Error", ex);
                }
            }
        }

        private class Startup
        {
            public readonly List<Action<ContainerBuilder>> ContainerActions = new List<Action<ContainerBuilder>>();

            private readonly Func<IContainer, IBusContext> addBusToContainerFunc;

            private readonly Func<IDocumentRepository> documentRepositoryFactory;

            private readonly Assembly domainLogicAssembly;

            private readonly Assembly domainModelsAssembly;

            private readonly IEnumerable<Assembly> handlerAssemblies = new[]
            {
                Assembly.GetEntryAssembly(),
                Assembly.GetExecutingAssembly()
            };

            private IContainer container;

            public Startup(
                Func<IContainer, IBusContext> addBusToContainerFunc,
                Func<IDocumentRepository> documentRepositoryFactory,
                Assembly domainLogicAssembly,
                Assembly domainModelsAssembly)
            {
                this.addBusToContainerFunc = addBusToContainerFunc;
                this.documentRepositoryFactory = documentRepositoryFactory;
                this.domainLogicAssembly = domainLogicAssembly;
                this.domainModelsAssembly = domainModelsAssembly;
            }

            public void Start(IApiCommand startupCommand)
            {
                var builder = EndpointSetup.ConfigureCore<TUserAuthenticator>(
                    new ContainerBuilder(),
                    this.domainLogicAssembly,
                    this.domainModelsAssembly,
                    this.handlerAssemblies,
                    MessageAggregator.Create,
                    this.documentRepositoryFactory,
                    this.ContainerActions);

                builder.RegisterInstance(EnvironmentConfig.Variables).AsSelf().As<IApplicationConfig>();

                builder.RegisterType<CommandHandler>().AsSelf().AsImplementedInterfaces().InstancePerDependency();

                this.container = builder.Build();

                EndpointSetup.ValidateContainer(this.container);

                this.addBusToContainerFunc(this.container); //will update

                Log.Logger.Information("Ready to receive messages");

                if (startupCommand != null) SendStartupMessage();

                void SendStartupMessage()
                {
                    this.container.Resolve<IBusContext>().SendLocal(new SendCommandOperation(startupCommand));
                }

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