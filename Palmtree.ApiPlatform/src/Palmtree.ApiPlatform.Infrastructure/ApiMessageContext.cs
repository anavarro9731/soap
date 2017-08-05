namespace Palmtree.ApiPlatform.Infrastructure
{
    using DataStore.Interfaces;
    using Serilog;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using Palmtree.ApiPlatform.Interfaces;

    public abstract class ApiMessageContext
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
