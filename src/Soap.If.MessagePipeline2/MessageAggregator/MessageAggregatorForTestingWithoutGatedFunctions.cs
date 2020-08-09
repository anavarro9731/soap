namespace Soap.MessagePipeline.MessageAggregator
{
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class MessageAggregatorForTestingWithoutGatedFunctions : MessageAggregatorForTestingBase, IMessageAggregator
    {
        public static MessageAggregatorForTesting Create() => new MessageAggregatorForTesting();

        public void Clear()
        {
            this.allMessages.Clear();
        }

        public void Remove(IMessage message)
        {
            this.allMessages.Remove(message);
        }

        public IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            this.allMessages.Add(message);
            return new MessagePropogator<TMessage>(message);
        }
    }
}