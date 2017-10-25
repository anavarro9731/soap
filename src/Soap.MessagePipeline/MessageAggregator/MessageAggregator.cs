namespace Soap.MessagePipeline.MessageAggregator
{
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class MessageAggregator : IMessageAggregator
    {
        private readonly ReadOnlyCapableList<IMessage> allMessages = new ReadOnlyCapableList<IMessage>();

        public IReadOnlyList<IMessage> AllMessages => this.allMessages;

        public static MessageAggregator Create()
        {
            return new MessageAggregator();
        }

        public void Collect(IMessage message)
        {
            this.allMessages.Add(message);
        }

        public virtual IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            this.allMessages.Add(message);
            return new MessagePropogator<TMessage>(message);
        }
    }
}