namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Bus;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages;
    using Soap.Utility.Functions.Operations;

    /// <summary>
    ///     represents a stateless multi-step process which occurs in a single unit of work
    ///     but involves more than one aggregate instance [regardless or type] and/or more than one service [e.g. EmailSender,
    ///     DataStore]
    ///     It records StatefulProcessStarted/Completed events whose purpose is mainly log instrumentation but could be used in
    ///     unit testing as well.
    /// </summary>

    public abstract class Process
    {
        private readonly ContextWithMessage context;

        protected Process(ContextWithMessage context)
        {
            this.context = context;
        }

        protected IDataStoreQueryCapabilities DataReader => this.context.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        protected ILogger Logger => this.context.Logger;

        protected Bus Bus { get; private set; }

        public async Task BeginProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            RecordStarted(new ProcessStarted(GetType().Name, meta.RequestedBy?.UserName));

            await process.BeginProcess(message, meta);

            RecordCompleted(new ProcessCompleted(GetType().Name, meta.RequestedBy?.UserName));
        }

        public async Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            RecordStarted(new ProcessStarted(GetType().Name, meta.RequestedBy?.UserName));

            var result = await process.BeginProcess(message, meta);

            RecordCompleted(new ProcessCompleted(GetType().Name, meta.RequestedBy?.UserName));

            return result;
        }

        private void RecordCompleted(ProcessCompleted processCompleted)
        {
            context.MessageAggregator.Collect(processCompleted);
        }

        private void RecordStarted(ProcessStarted statefulProcessStarted)
        {
            this.context.MessageAggregator.Collect(statefulProcessStarted);
        }
    }
}