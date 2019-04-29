namespace Soap.If.MessagePipeline.MessageAggregator
{
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;

    public class ValueReturner : IValueReturner
    {
        private readonly string eventType;

        private readonly Dictionary<string, Queue<object>> returnValues;

        public ValueReturner(Dictionary<string, Queue<object>> returnValues, string eventType)
        {
            this.returnValues = returnValues;
            this.eventType = eventType;
        }

        public IValueReturner Return<TReturnValue>(TReturnValue returnValue)
        {
            if (this.returnValues.ContainsKey(this.eventType))
            {
                this.returnValues[this.eventType].Enqueue(returnValue);
            }
            else
            {
                var newQueue = new Queue<object>();
                newQueue.Enqueue(returnValue);
                this.returnValues.Add(this.eventType, newQueue);
            }

            return new ValueReturner(this.returnValues, this.eventType);
        }
    }
}