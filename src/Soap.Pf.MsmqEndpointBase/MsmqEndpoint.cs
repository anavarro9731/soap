namespace Soap.Pf.MsmqEndpointBase
{
    using System;
    using System.Reflection;
    using Autofac;
    using DataStore.Interfaces;
    using Rebus.Autofac;
    using Rebus.Backoff;
    using Rebus.Config;
    using Rebus.Persistence.InMem;
    using Rebus.Retry.Simple;
    using Rebus.TransactionScopes;
    using Soap.If.Interfaces;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointClients;

    public static class MsmqEndpoint
    {
        public static MsmqEndpointConfiguration<TUserAuthenticator> Configure<TUserAuthenticator>(
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Action<IContainer> addBusToContainerFunc,
            Func<IDocumentRepository> buildDocumentRepositoryFunc) where TUserAuthenticator : IAuthenticateUsers
        {
            return new MsmqEndpointConfiguration<TUserAuthenticator>(
                domainLogicAssembly,
                domainMessagesAssembly,
                addBusToContainerFunc,
                buildDocumentRepositoryFunc);
        }

        public static void CreateBusContext(IApplicationConfig appConfig, IContainer container, params MsmqMessageRoute[] messageRoutes)
        {
            {
                var bus = new BusApiClient(messageRoutes, ConfigureRebus, 
                    Rebus.Config.Configure.With(new AutofacContainerAdapter(container)));

                //hotswap tx, ms msg storage

                bus.Start();

                RegisterBusAsIBusContext(bus);
            }

            /* IBusContext is our interface, where IBus is Rebus' */
            void RegisterBusAsIBusContext(BusApiClient busApiClient)
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterInstance(busApiClient).As<IBusContext>();
#pragma warning disable CS0618 // Type or member is obsolete
                containerBuilder.Update(container);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            void ConfigureRebus(RebusConfigurer configurer)
            {                
                configurer.Transport(t => t.UseMsmq(appConfig.ApiEndpointSettings.MsmqEndpointName))
                          .Options(
                              o =>
                                  {
                                  //not sure if this is necessary with our own
                                  //depends if this causes queue operations to enlist
                                  //but according to docs i don't think it does
                                  //need to check this and the messageconstraints.enforce
                                  //if it doesnt to ensure right behaviour
                                  //not used if swapped?
                                  o.HandleMessagesInsideTransactionScope();

                                  o.SimpleRetryStrategy($"{appConfig.ApiEndpointSettings.MsmqEndpointName}.error", appConfig.NumberOfApiMessageRetries + 1, false);

                                  //o.SetBackoffTimes(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1000));
                                  
                                  o.SetNumberOfWorkers(1);
                                  o.SetMaxParallelism(1);
                                  o.LogPipeline();
                                  })
                        .Subscriptions(s => s.StoreInMemory());
            }
        }
    }
}