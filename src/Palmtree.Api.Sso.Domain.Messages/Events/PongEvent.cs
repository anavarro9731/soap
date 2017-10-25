namespace Palmtree.Api.Sso.Domain.Messages.Events
{
    using System;
    using Soap.Interfaces.Messages;

    public class PongEvent : ApiEvent
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }
    }
}