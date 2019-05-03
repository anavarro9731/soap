    namespace Soap.Pf.MessageContractsBase.Events
{
    using System;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractPongEvent : ApiEvent
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public override void Validate()
        {

        }
    }
}