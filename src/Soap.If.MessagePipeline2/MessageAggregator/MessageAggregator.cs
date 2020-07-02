namespace Soap.MessagePipeline.MessageAggregator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class MessageAggregator : IMessageAggregator
    {
        private readonly List<IMessage> allMessages = new List<IMessage>();

        public void Clear()
        {
            this.allMessages.Clear();
        }

        public IReadOnlyList<IMessage> AllMessages => this.allMessages.AsReadOnly();

        public static MessageAggregator Create()
        {
            return new MessageAggregator();
        }

        public void Collect(IMessage message)
        {
            this.allMessages.Add(message);
        }

        public void RemoveWhere(Predicate<IMessage> predicate)
        {
            this.allMessages.RemoveAll(predicate);
        }

        public virtual IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            this.allMessages.Add(message);
            return new MessagePropogator<TMessage>(message);
        }
    }
}