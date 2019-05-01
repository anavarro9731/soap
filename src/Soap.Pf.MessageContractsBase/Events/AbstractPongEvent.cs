    namespace Soap.Pf.MessageContractsBase.Events
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class AbstractPongEvent : ApiEvent
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }
    }
}