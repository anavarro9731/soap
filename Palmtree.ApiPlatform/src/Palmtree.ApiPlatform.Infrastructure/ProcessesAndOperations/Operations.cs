namespace Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations
{
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using Palmtree.ApiPlatform.Interfaces;

    public class Operations<T> : Operations where T : class, IAggregate, new()
    {
        protected new IDataStoreWriteOnlyScoped<T> DataStore => base.DataStore.AsWriteOnlyScoped<T>();
    }

    public abstract class Operations : ApiMessageContext
    {
        protected IDataStoreQueryCapabilities DataStoreRead => DataStore.AsReadOnly();

        public new void SetDependencies(IDataStore dataStore, IUnitOfWork uow, ILogger logger, IMessageAggregator messageAggregator)
        {
            base.SetDependencies(dataStore, uow, logger, messageAggregator);
        }
    }
}
