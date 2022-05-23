namespace Soap.MessagePipeline.MessageAggregator
{
    using System;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Soap.Utility.Functions.Extensions;

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
                throw new CircuitException(
                    $@"Requested a return value from a CollectAndForward function while using the GatedMessagePropogator. 
                    But the object you have registered as a return value for this function is of the wrong type. 
                    You registered a return value of type {this.toReturn.GetType().ToTypeNameString()} but the function requires a type of {
                            typeof(TOut).ToTypeNameString()
                        }.                    
                    This could be because you have registered responses in the wrong order. 
                    Or perhaps you forgot to call .AsEnumerable() where the return type is IEnumerable. 
                    Or perhaps you forgot to wrap your response in a task e.g. Task.FromResult( {this.toReturn.GetType().ToTypeNameString()} ) if its async.",
                    e);
            }

            throw new CircuitException(
                $@"Requested a return value from a CollectAndForward function while using the GatedMessagePropogator. 
                But none has been set. Use the .When<{typeof(TMessage).ToTypeNameString()}>({typeof(TOut).ToTypeNameString()}) function to set one.");
        }
    }
}
