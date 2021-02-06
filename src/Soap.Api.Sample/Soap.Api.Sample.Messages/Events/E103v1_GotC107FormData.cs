namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Messages;

    public class E103v1_GotC107FormData : UIFormDataEvent
    {
        public override void Validate()
        {
        }
    }
}
