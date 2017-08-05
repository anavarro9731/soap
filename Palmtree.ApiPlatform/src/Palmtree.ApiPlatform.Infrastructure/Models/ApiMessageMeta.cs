namespace Palmtree.ApiPlatform.Infrastructure.Models
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Permissions;

    public class ApiMessageMeta
    {
        public MessageLogItem MessageLogItem { get; set; }

        public DateTime ReceivedAt { get; set; }

        public long ReceivedAtTimestamp { get; set; }

        public IUserWithPermissions RequestedBy { get; set; }

        public string Schema { get; set; }
    }
}
