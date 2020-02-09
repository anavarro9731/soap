namespace Soap.MessagePipeline.MessageAggregator
{
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.UnitOfWork;
    using Soap.Utility.Functions.Extensions;

    public abstract class MessageAggregatorForTestingBase
    {
        protected readonly ReadOnlyCapableList<IMessage> allMessages = new ReadOnlyCapableList<IMessage>();

        public IReadOnlyList<IMessage> AllMessages => this.allMessages;

        public IReadOnlyList<ApiCommand> CommandsSent => new ReadOnlyCapableList<ApiCommand>().Op(
            l => l.AddRange(AllMessages.OfType<QueuedApiCommand>().Select(co => co.Command).ToList()));

        public IReadOnlyList<IDataStoreOperation> DataStoreOperations => new ReadOnlyCapableList<IDataStoreOperation>().Op(
            l => l.AddRange(AllMessages.OfType<IDataStoreOperation>().ToList()));

        public IReadOnlyList<ApiEvent> EventsPublished => new ReadOnlyCapableList<ApiEvent>().Op(
            l => l.AddRange(AllMessages.OfType<QueuedApiEvent>().Select(peo => peo.Event).ToList()));

        public IReadOnlyList<ILogMessage> LogEntries => new ReadOnlyCapableList<ILogMessage>().Op(l => l.AddRange(AllMessages.OfType<ILogMessage>().ToList()));

        public IReadOnlyList<IQueuedStateChange> QueuedStateChanges => new ReadOnlyCapableList<IQueuedStateChange>().Op(
            l => l.AddRange(AllMessages.OfType<IQueuedStateChange>().ToList()));

        public IReadOnlyList<IStateOperation> StateOperations => new ReadOnlyCapableList<IStateOperation>().Op(
            l => l.AddRange(AllMessages.OfType<IStateOperation>().ToList()));

        public void Collect(IMessage message)
        {
            this.allMessages.Add(message);
        }
    }
}