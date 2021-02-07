namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Messages;

    public class E103v1_GotC107FormData : UIFormDataEvent<C107v1_CreateOrUpdateTestDataTypes>
    {
        public override void Validate()
        {
        }
    }
}
