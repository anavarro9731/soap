namespace Soap.Pf.EndpointInfrastructure
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;

    public abstract class MessageHandlerBase
    {

        protected IDataStore DataStore { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IMessageAggregator MessageAggregator { get; private set; }

        protected IUnitOfWork UnitOfWork { get; private set; }

        public void SetDependencies(IDataStore dataStore, IUnitOfWork uow, ILogger logger, IMessageAggregator messageAggregator)
        {
            UnitOfWork = uow;
            Logger = logger;
            MessageAggregator = messageAggregator;
            DataStore = dataStore;
        }
    }
}