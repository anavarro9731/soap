namespace Soap.Interfaces
{
    using System;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, ApiIdentity requestedBy)
        {
            ReceivedAt = receivedAt;
            RequestedBy = requestedBy;
        }

        public MessageMeta() {}
        

        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ApiIdentity RequestedBy { get; internal set; }
        
    }
}
