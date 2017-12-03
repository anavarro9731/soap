namespace Soap.If.MessagePipeline.Messages
{
    using System;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

    public class QueuedApiEvent : QueuedStateChange
    {
        public IApiEvent Event { get; set; }
    }

    public class PublishEventOperation : IPublishEventOperation
    {
        public PublishEventOperation(IApiEvent @event)
        {
            Event = @event;
        }

        public IApiEvent Event { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }
    }
}