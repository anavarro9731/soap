namespace Soap.Pf.EndpointInfrastructure
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.MessagePipeline.UnitOfWork;

    public abstract class MessageHandlerBase
    {
        protected IDataStore DataStore { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IMessageAggregator MessageAggregator { get; private set; }

        protected UnitOfWork UnitOfWork { get; private set; }

        public void SetDependencies(IDataStore dataStore, UnitOfWork uow, ILogger logger, IMessageAggregator messageAggregator)
        {
            UnitOfWork = uow;
            Logger = logger;
            MessageAggregator = messageAggregator;
            DataStore = dataStore;
        }
    }
}