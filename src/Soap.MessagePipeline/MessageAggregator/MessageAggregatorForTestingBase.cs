namespace Soap.MessagePipeline.MessageAggregator
{
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.Operations;
    using Soap.Bus;
    using Soap.Interfaces.Messages;

    public abstract class MessageAggregatorForTestingBase
    {
        protected readonly List<IMessage> allMessages = new List<IMessage>();

        public IReadOnlyList<IMessage> AllMessages => this.allMessages;

        public IReadOnlyList<ApiCommand> CommandsSent =>
            AllMessages.OfType<QueuedCommandToSend>().Select(co => co.CommandToSend).ToList().AsReadOnly();

        public IReadOnlyList<IDataStoreOperation> DataStoreOperations =>
            AllMessages.OfType<IDataStoreOperation>().ToList().AsReadOnly();

        public IReadOnlyList<ApiEvent> EventsPublished =>
            AllMessages.OfType<QueuedEventToPublish>().Select(peo => peo.EventToPublish).ToList().AsReadOnly();

        public IReadOnlyList<ILogMessage> LogEntries => AllMessages.OfType<ILogMessage>().ToList().AsReadOnly();

        public IReadOnlyList<IQueuedStateChange> QueuedStateChanges =>
            AllMessages.OfType<IQueuedStateChange>().ToList().AsReadOnly();

        public IReadOnlyList<IStateOperation> StateOperations => AllMessages.OfType<IStateOperation>().ToList().AsReadOnly();

        public void Collect(IMessage message)
        {
            this.allMessages.Add(message);
        }
    }
}