namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;

    public class Operations<T> : Operations where T : class, IAggregate, new()
    {
        protected IDataStoreWriteOnlyScoped<T> DataWriter => MContext.DataStore.AsWriteOnlyScoped<T>();
    }

    public abstract class Operations
    {
        protected IDataStoreQueryCapabilities DataReader => MContext.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => MContext.DataStore.WithoutEventReplay;

        protected ILogger Logger => MContext.Logger;

        protected IMessageAggregator MessageAggregator => MContext.MessageAggregator;

    }
}