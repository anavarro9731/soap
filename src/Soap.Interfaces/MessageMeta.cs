namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, ApiIdentity apiIdentity, string idChain)
        {
            ReceivedAt = receivedAt;
            ApiIdentity = apiIdentity;
            IdChain = idChain?.Split(',').ToList() ?? new List<string>();
        }

        public MessageMeta() {}
        

        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ApiIdentity ApiIdentity { get; internal set; }

        [JsonProperty]
        public List<string> IdChain { get; internal set; }
    }
}
