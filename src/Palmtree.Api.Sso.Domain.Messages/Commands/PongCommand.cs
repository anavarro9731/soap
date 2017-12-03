namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class PongCommand : ApiCommand
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }
    }
}