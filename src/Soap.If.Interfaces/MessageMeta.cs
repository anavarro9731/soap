namespace Soap.Interfaces
{
    using System;
    using Newtonsoft.Json;

    public class MessageMeta
    {
        public MessageMeta((DateTime receivedAt, long receivedAtTick) receivedAt, IApiIdentity requestedBy, string schema)
        {
            ReceivedAt = receivedAt;
            RequestedBy = requestedBy;
            Schema = schema;
        }

        public MessageMeta()
        {
            //- serialiser
        }

        [JsonProperty]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonProperty]
        public IApiIdentity RequestedBy { get; internal set; }

        [JsonProperty]
        public string Schema { get; internal set; }
    }
}