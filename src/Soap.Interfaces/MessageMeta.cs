namespace Soap.Interfaces
{
    using System;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, ApiIdentity apiIdentity)
        {
            ReceivedAt = receivedAt;
            ApiIdentity = apiIdentity;
        }

        public MessageMeta() {}
        

        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ApiIdentity ApiIdentity { get; internal set; }
        
    }
}
