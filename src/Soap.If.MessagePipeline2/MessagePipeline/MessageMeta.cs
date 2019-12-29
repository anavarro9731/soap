namespace Soap.If.MessagePipeline.Models
{
    using System;
    using CircuitBoard.Permissions;
    using Soap.If.MessagePipeline.Models.Aggregates;

    public class MessageMeta
    {
        public MessageLogEntry MessageLogItem { get; set; }

        public DateTime ReceivedAt { get; set; }

        public long StartTicks { get; set; }

        public IIdentityWithPermissions RequestedBy { get; set; }

        public string Schema { get; set; }

    }
}