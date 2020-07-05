namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Context;

    public class Operations<T> : IOperation where T : class, IAggregate, new()
    {
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        public DataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        public DataStoreWriteOnly<T> DataWriter => this.context.DataStore.AsWriteOnlyScoped<T>();

        public IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        public ILogger Logger => this.context.Logger;

    }
}