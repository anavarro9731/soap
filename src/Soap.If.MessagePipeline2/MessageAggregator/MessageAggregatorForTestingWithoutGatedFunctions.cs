namespace Soap.If.MessagePipeline.MessageAggregator
{
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class MessageAggregatorForTestingWithoutGatedFunctions : MessageAggregatorForTestingBase, IMessageAggregator
    {
        public static MessageAggregatorForTesting Create()
        {
            return new MessageAggregatorForTesting();
        }

        public IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            this.allMessages.Add(message);
            return new MessagePropogator<TMessage>(message);
        }

        public void Clear()
        {
            this.allMessages.Clear();
        }
    }
}