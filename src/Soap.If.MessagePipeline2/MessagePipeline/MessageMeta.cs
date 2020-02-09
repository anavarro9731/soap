namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using CircuitBoard.Permissions;

    public class MessageMeta
    {
        public DateTime ReceivedAt { get; set; }

        public long StartTicks { get; set; }

        public IIdentityWithPermissions RequestedBy { get; set; }

        public string Schema { get; set; }

    }
}