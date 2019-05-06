﻿namespace Soap.If.MessagePipeline.ProcessesAndOperations
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.If.MessagePipeline.UnitOfWork;

    public class Operations<T> : Operations where T : class, IAggregate, new()
    {
        protected new IDataStoreWriteOnlyScoped<T> DataStore => base.DataStore.AsWriteOnlyScoped<T>();
    }

    public abstract class Operations
    {
        protected IDataStore DataStore { get; private set; }

        protected IDataStoreQueryCapabilities DataStoreRead => DataStore.AsReadOnly();

        protected IWithoutEventReplay DataStoreReadWithoutEventReplay => DataStore.WithoutEventReplay;

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