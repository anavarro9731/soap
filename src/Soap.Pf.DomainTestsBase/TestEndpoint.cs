namespace Soap.Pf.DomainTestsBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Autofac;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessageAggregator;
    using Soap.If.MessagePipeline.MessagePipeline;

    public class TestEndpoint
    {
        private readonly IContainer container;

        public TestEndpoint(IContainer container)
        {
            this.container = container;
        }

        public IApplicationConfig AppConfig => this.container.Resolve<IApplicationConfig>();

        public InMemoryMessageBus InMemoryMessageBus => (InMemoryMessageBus)this.container.Resolve<IBusContext>();

        public MessageAggregatorForTesting MessageAggregator => (MessageAggregatorForTesting)this.container.Resolve<IMessageAggregator>();

        protected InMemoryDocumentRepository InMemoryDocumentRepository => (InMemoryDocumentRepository)this.container.Resolve<IDocumentRepository>();

        public static TestEndpointConfiguration<TUserAuthenticator> Configure<TUserAuthenticator>(
            Assembly domainLogicAssembly,
            Assembly domainMessagesAssembly,
            Assembly msmqEndpointAssembly,
            Assembly httpEndpointAssembly,
            IApplicationConfig applicationConfig) where TUserAuthenticator : IAuthenticateUsers
        {
            return new TestEndpointConfiguration<TUserAuthenticator>(
                domainLogicAssembly,
                domainMessagesAssembly,
                msmqEndpointAssembly,
                httpEndpointAssembly,
                applicationConfig);
        }

        public void AddToDatabase<T>(T aggregate) where T : IAggregate
        {
            InMemoryDocumentRepository.Aggregates.Add(aggregate);
        }

        public void HandleCommand(ApiCommand command)
        {
            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            this.container.Resolve<MessagePipeline>().Execute(command).GetAwaiter().GetResult();
        }

        public T HandleCommand<T>(ApiCommand<T> command) where T : class, new()
        {
            if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

            var result = this.container.Resolve<MessagePipeline>().Execute(command).GetAwaiter().GetResult();

            return (T)result;
        }

        public void HandleQuery(ApiQuery query)
        {
            if (query.MessageId == Guid.Empty) query.MessageId = Guid.NewGuid();

            this.container.Resolve<MessagePipeline>().Execute(query).GetAwaiter().GetResult();
        }

        public T HandleQuery<T>(ApiQuery<T> query) where T : class, new()
        {
            if (query.MessageId == Guid.Empty) query.MessageId = Guid.NewGuid();

            var result = this.container.Resolve<MessagePipeline>().Execute(query).GetAwaiter().GetResult();

            return (T)result;
        }

        public Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : IHaveSchema, IHaveAUniqueId
        {
            var queryResult = extendQueryable == null
                                  ? InMemoryDocumentRepository.Aggregates.OfType<T>()
                                  : extendQueryable(InMemoryDocumentRepository.Aggregates.OfType<T>().AsQueryable());
            return Task.FromResult(queryResult);
        }
    }
}