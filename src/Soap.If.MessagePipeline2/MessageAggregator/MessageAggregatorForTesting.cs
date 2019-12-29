namespace Soap.If.MessagePipeline.MessageAggregator
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;

    /// <summary>
    ///     doesn't allow calls through unless they are DataStore or Bus operations which we always test
    ///     in a state-based way with fakes
    /// </summary>
    public class MessageAggregatorForTesting : MessageAggregatorForTestingBase, IMockGatedFunctions, IMessageAggregator
    {
        public Dictionary<string, Queue<object>> ReturnValues = new Dictionary<string, Queue<object>>();

        public static MessageAggregatorForTesting Create()
        {
            return new MessageAggregatorForTesting();
        }

        public IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            this.allMessages.Add(message);

            if (message is IDataStoreOperation) //we never want to gate these in testing because we are collecting calls in a "FakeStore" rather than setting up return values
            {
                return new MessagePropogator<TMessage>(message);
            }

            var eventType = typeof(TMessage).FullName;

            if (this.ReturnValues.ContainsKey(eventType) && this.ReturnValues[eventType].Count == 0)
            {
                throw new Exception(
                    $@"Requested a return value from a CollectAndForward function while using the GatedMessagePropogator. 
                    The return value has been registered by a call to When<{
                            typeof(TMessage).Name
                        }>(), but the production code requests a return value more times then 
                    you have registered responses. Please register additional return values.");
            }

            return new GatedMessagePropogator<TMessage>(message, this.ReturnValues.ContainsKey(eventType) ? this.ReturnValues[eventType].Dequeue() : null);
        }

        public IValueReturner When<TMessage>() where TMessage : IMessage
        {
            return new ValueReturner(this.ReturnValues, typeof(TMessage).FullName);
        }
    }
}