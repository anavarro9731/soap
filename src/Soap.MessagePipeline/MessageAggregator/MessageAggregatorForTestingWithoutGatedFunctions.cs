namespace Soap.MessagePipeline.MessageAggregator
{
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages;

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
    }
}
