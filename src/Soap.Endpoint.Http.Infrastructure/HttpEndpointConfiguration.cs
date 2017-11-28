namespace Soap.Endpoint.Http.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using DataStore.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Serialization;
    using Serilog;
    using Soap.Endpoint.Clients;
    using Soap.Endpoint.Infrastructure;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessageAggregator;

    public class HttpEndpointConfiguration<TUserAuthenticator> where TUserAuthenticator : IAuthenticateUsers
    {
        private static readonly List<Action<ContainerBuilder>> containerActions = new List<Action<ContainerBuilder>>();

        private static readonly IEnumerable<Assembly> handlerAssemblies = new[]
        {
            Assembly.GetEntryAssembly(),
            Assembly.GetExecutingAssembly()
        };

        private static Func<IBusContext> busContextFactory;

        private static Func<IDocumentRepository> documentRepositoryFactory;

        private static Assembly domainLogicAssembly;

        private static Assembly domainModelsAssembly;

        private static ILogger logger;

        private static IApiCommand startupCommand;

        public HttpEndpointConfiguration(
            Assembly domainLogicAssembly,
            Assembly domainModelsAssembly,
            Func<IBusContext> busContextFactory,
            Func<IDocumentRepository> documentRepositoryFactory)
        {
            HttpEndpointConfiguration<TUserAuthenticator>.domainLogicAssembly = domainLogicAssembly;
            HttpEndpointConfiguration<TUserAuthenticator>.domainModelsAssembly = domainModelsAssembly;
            HttpEndpointConfiguration<TUserAuthenticator>.busContextFactory = busContextFactory;
            HttpEndpointConfiguration<TUserAuthenticator>.documentRepositoryFactory = documentRepositoryFactory;

            try
            {
                CreateLogger(out logger);
            }
            catch (Exception ex)
            {
                //add fail-safe sources only                        
                Trace.TraceError($"Startup Error {ex}");
                logger?.Error(ex, "Startup Error");

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

        private static IHttpEnvironmentSpecificConfiguration EnvironmentConfig => (IHttpEnvironmentSpecificConfiguration)EndpointSetup.GetConfiguration();

        public HttpEndpointConfiguration<TUserAuthenticator> ConfigureContainer(Action<ContainerBuilder> configureContainerAction)
        {
            if (configureContainerAction != null) containerActions.Add(configureContainerAction);

            return this;
        }

        public void Start(IApiCommand startupCommand = null)
        {
            {
                try
                {
                    if (startupCommand != null) HttpEndpointConfiguration<TUserAuthenticator>.startupCommand = startupCommand;

                    if (Environment.UserInteractive) Console.Title = EnvironmentConfig.Variables.ApplicationName;

                    var apiHttpUrl = EnvironmentConfig.Variables.ApiEndpointSettings.HttpEndpointUrl;

                    var builder = new WebHostBuilder().UseKestrel()
                                                      .UseContentRoot(Directory.GetCurrentDirectory())
                                                      .UseIISIntegration()
                                                      .UseUrls(apiHttpUrl)
                                                      .UseStartup<Startup>();
                    var host = builder.Build();

                    host.Run();

                    Log.Logger.Information("No longer receiving messages");
                    Log.CloseAndFlush();
                }
                catch (Exception ex)
                {
                    //add fail-safe sources only                        
                    Trace.TraceError($"Startup Error {ex}");
                    logger.Error(ex, "Startup Error");

                    //Prevent the app continuing if the error occurs during startup
                    throw new Exception("Startup Error", ex);
                }
            }
        }

        public class Startup
        {
            /* This method gets called by the runtime. Order #2
             * Essentially we use this for things which run after the container is setup.
             * or for things MS has designed to be called here (e.g. appBuilder.useXXXX() methods) 
             * "Configure" is an unhelpful name [See comment on ConfigureServices()].
             * 
             * See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup for details
             * 
             */
            public void Configure(
                IApplicationBuilder applicationBuilder,
                ILifetimeScope rootLifetimeScope, //if needed this is the autofac container
                ILogger logger)
            {
                {
                    ConfigureCorsPolicy(applicationBuilder); // must precede configwebserver

                    ConfigureWebServer(applicationBuilder);

                    LogEndpointReady();

                    if (startupCommand != null) SendStartupCommand();
                }

                void LogEndpointReady()
                {
                    logger.Information("Ready to receive messages");
                }

                void ConfigureCorsPolicy(IApplicationBuilder appBuilder)
                {
                    appBuilder.UseCors(builder => { EnvironmentConfig.DefineCorsPolicyPerEnvironment(builder); });
                }

                void ConfigureWebServer(IApplicationBuilder appBuilder)
                {
                    appBuilder.UseMvc();
#if DEBUG
                    //this was required to make the endpoint work with some library or config setting we 
                    //were using but it's not functionally required
                    appBuilder.UseDeveloperExceptionPage();
#endif
                }

                void SendStartupCommand()
                {
                    var bus = rootLifetimeScope.Resolve<IBusContext>();
                    bus.SendLocal(new SendCommandOperation(startupCommand));
                }
            }



            /* This method gets called by the runtime. Order #1
             * Use this method to add "services" to the built-in aspnetContainer
             * I completely agree with Frans Bouma's comment here: http://wildermuth.com/2015/3/2/A_Look_at_ASP_NET_5_Part_2_-_Startup 
             * This method should not have been split from Configure because there is no clear delineation between the two "configure methods".
             * In order to add an item you often need to configure it first. With a custom container this is definately true.
             */
            public IServiceProvider ConfigureServices(IServiceCollection serviceCollection)
            {
                {
                    AddInMemoryCaching(serviceCollection);
                    AddMvcFramework(serviceCollection);
                    AddCorsFramework(serviceCollection);

                    CopyItemFromAspNetContainerToAutofacBuilder(serviceCollection, out ContainerBuilder builder);

                    EndpointSetup.ConfigureCore<TUserAuthenticator>(
                        builder,
                        domainLogicAssembly,
                        domainModelsAssembly,
                        handlerAssemblies,
                        MessageAggregator.Create,
                        documentRepositoryFactory,
                        containerActions);

                    builder.RegisterInstance(EnvironmentConfig.Variables).AsSelf().As<IApplicationConfig>();

                    //we want to build this here even though we could pass in an instance
                    //because we want to ensure that the serilog global logger has been setup
                    builder.RegisterInstance(busContextFactory()).As<IBusContext>();

                    var container = builder.Build();

                    EndpointSetup.ValidateContainer(container);

                    return AutofacContainerAsIServiceProvider(container);
                }

                IServiceProvider AutofacContainerAsIServiceProvider(IComponentContext container)
                {
                    return new AutofacServiceProvider(container);
                }

                void AddCorsFramework(IServiceCollection aspnetContainer)
                {
                    aspnetContainer.AddCors(); // for web server
                }

                void AddInMemoryCaching(IServiceCollection aspnetContainer)
                {
                    aspnetContainer.AddMemoryCache();
                }

                void CopyItemFromAspNetContainerToAutofacBuilder(IServiceCollection aspnetContainer, out ContainerBuilder builder)
                {
                    builder = new ContainerBuilder();
                    builder.Populate(aspnetContainer);
                }

                void AddMvcFramework(IServiceCollection aspnetContainer)
                {
                    aspnetContainer.AddMvc(mvcOptions => { })
                                   .AddJsonOptions(
                                       options =>
                                           {
                                               // force WebApi to serialise in camelCase
                                               options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                           });
                }
            }
        }
    }
}