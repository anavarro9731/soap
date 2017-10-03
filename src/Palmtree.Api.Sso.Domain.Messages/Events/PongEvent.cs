namespace Palmtree.Api.Sso.Domain.Messages.Events
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class PongEvent : ApiEvent
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }
    }
}
