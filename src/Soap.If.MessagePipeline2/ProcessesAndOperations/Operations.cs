namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.MessagePipeline.Context;

    public class Operations<T> : Operations where T : class, IAggregate, new()
    {
        private readonly ContextWithMessageLogEntry context;

        public Operations(ContextWithMessageLogEntry context)
            : base(context)
        {
            this.context = context;
        }

        protected IDataStoreWriteOnlyScoped<T> DataWriter => this.context.DataStore.AsWriteOnlyScoped<T>();
    }

    public abstract class Operations
    {
        private readonly ContextWithMessageLogEntry context;

        protected Operations(ContextWithMessageLogEntry context)
        {
            this.context = context;
        }

        protected IDataStoreQueryCapabilities DataReader => this.context.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        protected ILogger Logger => this.context.Logger;

        protected IMessageAggregator MessageAggregator => this.context.MessageAggregator;
    }
}