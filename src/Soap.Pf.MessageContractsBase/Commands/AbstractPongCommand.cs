namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractPongCommand : ApiCommand
    {
        public  DateTime PingedAt { get; set; }

        public  string PingedBy { get; set; }
    }
}