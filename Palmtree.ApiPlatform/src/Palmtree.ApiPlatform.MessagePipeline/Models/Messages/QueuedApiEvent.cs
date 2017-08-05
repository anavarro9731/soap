namespace Palmtree.ApiPlatform.MessagePipeline.Models.Messages
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Interfaces;

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
