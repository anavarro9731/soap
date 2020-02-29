namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using CircuitBoard.Permissions;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, IIdentityWithPermissions requestedBy, string schema)
        {
            ReceivedAt = receivedAt;
            RequestedBy = requestedBy;
            Schema = schema;
        }

        internal MessageMeta() { }

        public (DateTime DateTime, long Ticks) ReceivedAt { get; set; }

        public IIdentityWithPermissions RequestedBy { get; set; }

        public string Schema { get; set; }

    }
}