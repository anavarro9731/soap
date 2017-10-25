namespace Soap.MessagePipeline.MessageAggregator
{
    using System;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class GatedMessagePropogator<TMessage> : IPropogateMessages<TMessage> where TMessage : IMessage
    {
        private readonly object toReturn;

        internal GatedMessagePropogator(TMessage message, object toReturn)
        {
            this.toReturn = toReturn;
        }

        public void To(Action<TMessage> passTo)
        {
        }

        public TOut To<TOut>(Func<TMessage, TOut> passTo)
        {
            try
            {
                if (this.toReturn != null) return (TOut)this.toReturn;
            }
            catch (InvalidCastException e)
            {
                throw new Exception(
                    $@"Requested a return value from a CollectAndForward function while using the GatedMessagePropogator. 
                    But the object you have registered as a return value for this function is of the wrong type. 
                    You registered a return value of type {this.toReturn.GetType().Name} but the function requires a type of {
                            typeof(TOut).Name
                        }.                    
                    This could be because you have registered responses in the wrong order. 
                    Or perhaps you forgot to call .AsEnumerable() where the return type is IEnumerable. 
                    Or perhaps you forgot to wrap your response in a Task<{typeof(TOut).Name}> if its async.",
                    e);
            }

            throw new Exception(
                $@"Requested a return value from a CollectAndForward function while using the GatedMessagePropogator. 
                But none has been set. Use the .When<{typeof(TMessage).Name}>({typeof(TOut).Name}) function to set one.");
        }
    }
}