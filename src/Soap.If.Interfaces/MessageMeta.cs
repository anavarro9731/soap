namespace Soap.Interfaces
{
    using System;
    using System.Text.Json.Serialization;

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

        [JsonInclude]
        public (DateTime DateTime, long Ticks) ReceivedAt { get; internal set; }

        [JsonInclude]
        public IApiIdentity RequestedBy { get; internal set; }

        [JsonInclude]
        public string Schema { get; internal set; }

    }
}