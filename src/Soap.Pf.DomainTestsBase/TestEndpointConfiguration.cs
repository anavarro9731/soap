namespace Soap.Pf.DomainTestsBase
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using DataStore;
    using Destructurama;
    using Serilog;
    using Serilog.Exceptions;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.MessageAggregator;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.EndpointInfrastructure;
    using Soap.Pf.HttpEndpointBase;
    using Soap.Pf.MsmqEndpointBase;

    public class TestEndpointConfiguration<TUserAuthenticator> where TUserAuthenticator : IAuthenticateUsers
    {
        private readonly List<Action<IApplicationConfig>> appConfigActions = new List<Action<IApplicationConfig>>();

        private readonly IApplicationConfig applicationConfig;

        private readonly List<Action<ContainerBuilder>> containerActions = new List<Action<ContainerBuilder>>();

        private readonly Assembly domainLogicAssembly;

        private readonly Assembly domainMessagesAssembly;

        private readonly Assembly httpEndpointAssembly;

        private readonly Assembly msmqEndpointAssembly;

        public TestEndpointConfiguration(
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Assembly msmqEndpointAssembly,
            Assembly httpEndpointAssembly,
            IApplicationConfig applicationConfig = null)
        {
            this.domainLogicAssembly = domainLogicAssembly;
            this.domainMessagesAssembly = domainMessagesAssembly;
            this.msmqEndpointAssembly = msmqEndpointAssembly;
            this.httpEndpointAssembly = httpEndpointAssembly;
            this.applicationConfig = applicationConfig;
        }

        public TestEndpointConfiguration<TUserAuthenticator> ConfigureApplicationConfig(Action<IApplicationConfig> configureApplicationConfigurationAction)
        {
            if (configureApplicationConfigurationAction != null) this.appConfigActions.Add(configureApplicationConfigurationAction);
            return this;
        }

        public TestEndpointConfiguration<TUserAuthenticator> ConfigureContainer(Action<ContainerBuilder> configureContainerAction)
        {
            if (configureContainerAction != null) this.containerActions.Add(configureContainerAction);
            return this;
        }

        public TestEndpoint Start()
        {
            {
                ILogger logger = null;

                try
                {
                    var builder = new ContainerBuilder();

                    var messageAggregator = MessageAggregatorForTesting.Create();
                    var sink = new MessageAggregatorSink(messageAggregator);

                    CreateLogger(sink, out logger);

                    EndpointSetup.ConfigureCore<TUserAuthenticator>(
                        builder,
                        this.applicationConfig,
                        new [] { this.domainLogicAssembly, SoapPfDomainLogicBase.GetAssembly}.ToList(),
                        new [] { this.domainMessagesAssembly }.ToList(),
                        () => messageAggregator,
                        this.containerActions);

                    ApplyCustomApplicationConfigActions();

                    builder.RegisterInstance(new InMemoryMessageBus()).As<IBusContext>();


                    MsmqEndpointConfiguration<TUserAuthenticator>.Startup.AddHandlers(
                        builder,
                        new[]
                        {
                            this.msmqEndpointAssembly
                        });
                    HttpEndpointConfiguration<TUserAuthenticator>.Startup.AddHandlers(
                        builder,
                        new[]
                        {
                            this.httpEndpointAssembly
                        });

                    var container = builder.Build();

                    EndpointSetup.ValidateContainer(container);

                    return new TestEndpoint(container);
                }
                catch (Exception ex)
                {
                    //add fail-safe sources only                        
                    Trace.TraceError($"Startup Error {ex}");
                    logger?.Error(ex, "Startup Error {Message}", ex.Message);

                    //Prevent the app continuing if the error occurs during startup
                    throw new Exception("Startup Error", ex);
                }
            }

            void CreateLogger(MessageAggregatorSink sink, out ILogger logger)
            {
                var loggerConfiguration = new LoggerConfiguration().Enrich.WithProperty("Environment", "Testing")
                                                                   .Enrich.WithExceptionDetails()
                                                                   .Destructure.UsingAttributes()
                                                                   .WriteTo.Sink(sink)
                                                                   .WriteTo.Console();

                logger = loggerConfiguration.CreateLogger(); // create serilog ILogger
                Log.Logger = logger; //set serilog default instance which is expected by most serilog plugins
            }

            void ApplyCustomApplicationConfigActions()
            {
                this.appConfigActions.ForEach(a => a(this.applicationConfig));
            }
        }
    }
}