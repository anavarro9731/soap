namespace Soap.Pf.EndpointInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public static class EndpointSetup
    {
        public static ContainerBuilder ConfigureCore<TUserAuthenticator>(
            ContainerBuilder builder,
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Func<IMessageAggregator> messageAggregatorFactory,
            Func<IDocumentRepository> documentRepositoryFactory,
            List<Action<ContainerBuilder>> containerActions) where TUserAuthenticator : IAuthenticateUsers
        {
            {
                AddLogging();
                AddMessageAggregator();
                //adding api messages to find message frm sp controller
                AddUnitOfWorkAndCoreServices();
                AddMessageAuthenticator();
                AddOperations();
                AddProcesses();
                AddMessagePipeline();
                ApplyCustomActions();

                return builder;
            }

            void ApplyCustomActions()
            {
                containerActions.ForEach(a => a(builder));
            }

            void AddMessageAuthenticator()
            {
                builder.RegisterType<TUserAuthenticator>().As<IAuthenticateUsers>();
            }

            void AddUnitOfWorkAndCoreServices()
            {
                builder.Register(c => documentRepositoryFactory()).AsSelf().As<IDocumentRepository>().InstancePerLifetimeScope();
                builder.RegisterType<DataStore>().AsSelf().As<IDataStore>().InstancePerLifetimeScope();
                builder.RegisterType<QueuedStateChanger>().AsSelf().InstancePerLifetimeScope();
                builder.RegisterType<UnitOfWork>().As<UnitOfWork>().InstancePerLifetimeScope();
            }

            void AddMessageAggregator()
            {
                builder.Register(c => messageAggregatorFactory()).AsSelf().As<IMessageAggregator>().InstancePerLifetimeScope();
            }

            void AddLogging()
            {
                builder.RegisterInstance(Log.Logger).AsSelf().As<ILogger>();
            }

            void AddMessagePipeline()
            {
                builder.RegisterType<MessagePipeline>().AsSelf().As<MessagePipeline>();
            }

            void AddProcesses()
            {
                builder.RegisterAssemblyTypes(domainLogicAssembly)
                       .AssignableTo(typeof(Process))
                       .AsImplementedInterfaces()
                       .OnActivated(
                           e =>
                               {
                                   (e.Instance as Process).SetDependencies(
                                       e.Context.Resolve<IDataStore>(),
                                       e.Context.Resolve<UnitOfWork>(),
                                       e.Context.Resolve<ILogger>(),
                                       e.Context.Resolve<IMessageAggregator>());
                               })
                       .InstancePerDependency();

                builder.RegisterAssemblyTypes(domainLogicAssembly)
                       .AssignableTo(typeof(StatefulProcess))
                       .AsImplementedInterfaces()
                       .OnActivated(
                           e =>
                               {
                                   (e.Instance as StatefulProcess).SetDependencies(
                                       e.Context.Resolve<IDataStore>(),
                                       e.Context.Resolve<UnitOfWork>(),
                                       e.Context.Resolve<ILogger>(),
                                       e.Context.Resolve<IMessageAggregator>());
                               })
                       .InstancePerDependency();
            }

            void AddOperations()
            {
                builder.RegisterAssemblyTypes(domainLogicAssembly)
                       .AsClosedTypesOf(typeof(Operations<>))
                       .OnActivated(
                           e =>
                               {
                                   (e.Instance as Operations).SetDependencies(
                                       e.Context.Resolve<IDataStore>(),
                                       e.Context.Resolve<UnitOfWork>(),
                                       e.Context.Resolve<ILogger>(),
                                       e.Context.Resolve<IMessageAggregator>());
                               })
                       .InstancePerLifetimeScope();
            }
        }

        public static IEnvironmentSpecificConfig GetConfiguration()
        {
            const string AppSettingKey = "Soap.Environment";

            var currentEnvironment = ConfigurationManager.AppSettings["Soap.Environment"];

            if (string.IsNullOrEmpty(currentEnvironment))
            {
                throw new Exception(
                    "No startup configuration defined. "
                    + $"You must add an App.Config file with an <appSetting> element whose key is '{AppSettingKey}' and whose value matches a class "
                    + "in the endpoint which implements IEnvironmentSpecificConfig.");
            }

            var environmentConfigClasses = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()) //unit tests 
                                           .GetTypes()
                                           .Where(t => t.InheritsOrImplements(typeof(IEnvironmentSpecificConfig)) && !t.IsInterface && !t.IsAbstract);

            foreach (var environmentConfigClass in environmentConfigClasses)
                if (string.Equals(environmentConfigClass.Name, currentEnvironment, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (IEnvironmentSpecificConfig)Activator.CreateInstance(environmentConfigClass);
                }

            throw new Exception(
                $"No Environment Config class defined for '{currentEnvironment}' environment. "
                + $"You must add a class to your endpoint called '{currentEnvironment}' which implements IEnvironmentSpecificConfig.");
        }

        public static void ValidateContainer(IContainer container)
        {
            {
                AssertOnlySingleHandlerPerMessageRegistrations();
            }

            void AssertOnlySingleHandlerPerMessageRegistrations()
            {
                {
                    var invalidHandlerTypeNamesListed = string.Join(
                        " | ",
                        container.Resolve<IList<IMessageHandler>>()
                                 .Select(GetMessageHandlerType)
                                 .Where(IsDerivedFromGenericMessageHandlerType)
                                 .GroupBy(GetMessageType)
                                 .Where(DoesMessageTypeHaveMultipleHandlers)
                                 .SelectMany(
                                     g => g.Select(
                                         handlerType => new
                                         {
                                             HandlerType = handlerType.AsTypeNameString(),
                                             BaseMessageHandlerType = handlerType.BaseType.AsTypeNameString(),
                                             MessageType = g.Key.AsTypeNameString()
                                         }))
                                 .OrderBy(x => x.MessageType)
                                 .ThenBy(x => x.HandlerType)
                                 .Select(x => $"{x.HandlerType} : {x.BaseMessageHandlerType}")
                                 .ToList());

                    Guard.Against(
                        string.IsNullOrWhiteSpace(invalidHandlerTypeNamesListed) == false,
                        $"One or more handlers are registered to handler the same message type: {invalidHandlerTypeNamesListed}");
                }

                Type GetMessageHandlerType(IMessageHandler handler)
                {
                    return handler.GetType();
                }

                bool IsDerivedFromGenericMessageHandlerType(Type handlerType)
                {
                    return handlerType.BaseType?.GenericTypeArguments.FirstOrDefault() != null;
                }

                Type GetMessageType(Type handlerType)
                {
                    return handlerType.BaseType?.GenericTypeArguments.First();
                }

                bool DoesMessageTypeHaveMultipleHandlers(IGrouping<Type, Type> messageTypePerHandlerTypes)
                {
                    return messageTypePerHandlerTypes.Count() > 1;
                }
            }
        }
    }
}