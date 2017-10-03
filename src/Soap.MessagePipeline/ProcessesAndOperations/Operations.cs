namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using Soap.Interfaces;

    public class Operations<T> : Operations where T : class, IAggregate, new()
    {
        protected new IDataStoreWriteOnlyScoped<T> DataStore => base.DataStore.AsWriteOnlyScoped<T>();
    }

    public abstract class Operations
    {
        protected IDataStore DataStore { get; private set; }

        protected IDataStoreQueryCapabilities DataStoreRead => DataStore.AsReadOnly();

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