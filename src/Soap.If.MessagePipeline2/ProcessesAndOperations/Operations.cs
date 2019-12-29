namespace Soap.If.MessagePipeline.ProcessesAndOperations
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.MessagePipeline2.MessagePipeline;

    public class Operations<T> : Operations where T : class, IAggregate, new()
    {
        protected IDataStoreWriteOnlyScoped<T> DataWriter => MMessageContext.DataStore.AsWriteOnlyScoped<T>();
    }

    public abstract class Operations
    {
        protected IDataStoreQueryCapabilities DataReader => MMessageContext.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => MMessageContext.DataStore.WithoutEventReplay;

        protected ILogger Logger => MMessageContext.Logger;

        protected IMessageAggregator MessageAggregator => MMessageContext.MessageAggregator;

    }
}