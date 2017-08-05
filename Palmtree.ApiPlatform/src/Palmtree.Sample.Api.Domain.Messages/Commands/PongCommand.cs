namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class PongCommand : ApiCommand
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }
    }
}
