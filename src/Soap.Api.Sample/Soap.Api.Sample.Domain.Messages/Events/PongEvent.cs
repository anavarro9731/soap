namespace Soap.Api.Sample.Domain.Messages.Events
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class PongEvent : ApiEvent
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }
    }
}