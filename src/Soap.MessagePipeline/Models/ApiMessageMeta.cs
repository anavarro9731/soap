namespace Soap.MessagePipeline.Models
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Soap.MessagePipeline.Models.Aggregates;

    public class ApiMessageMeta
    {
        public MessageLogItem MessageLogItem { get; set; }

        public DateTime ReceivedAt { get; set; }

        public long ReceivedAtTimestamp { get; set; }

        public IUserWithPermissions RequestedBy { get; set; }

        public string Schema { get; set; }
    }
}
