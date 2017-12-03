namespace Soap.If.MessagePipeline.MessageAggregator
{
    using System.Linq;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions.Extensions;

    public abstract class MessageAggregatorForTestingBase
    {
        protected readonly ReadOnlyCapableList<IMessage> allMessages = new ReadOnlyCapableList<IMessage>();

        public IReadOnlyList<IMessage> AllMessages => this.allMessages;

        public IReadOnlyList<IApiCommand> CommandsSent => new ReadOnlyCapableList<IApiCommand>().Op(
            l => l.AddRange(AllMessages.OfType<ISendCommandOperation>().Select(co => co.Command).ToList()));

        public IReadOnlyList<IDataStoreOperation> DataStoreOperations => new ReadOnlyCapableList<IDataStoreOperation>().Op(
            l => l.AddRange(AllMessages.OfType<IDataStoreOperation>().ToList()));

        public IReadOnlyList<IApiEvent> EventsPublished => new ReadOnlyCapableList<IApiEvent>().Op(
            l => l.AddRange(AllMessages.OfType<IPublishEventOperation>().Select(peo => peo.Event).ToList()));

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