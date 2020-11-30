namespace Soap.Api.Sample.Messages.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Soap.Pf.MessageContractsBase;

    public class E500v1_GetC107Form : UIFormDataEvent
    {

        private readonly List<Enumeration> postCodes = new List<Enumeration>()
        {
            new Enumeration("hr20dn", "HR2 0DN"),
            new Enumeration("al55ng", "AL55NG"),
            new Enumeration("ox29ju", "OX2 9JU")
        };
        
        protected override ApiCommand UserDefinedValues() => new C107v1_TestDataTypes()
        {
            C107_String = "test default",
            C107_PostCodes = new EnumerationAndFlags(this.postCodes.First(), this.postCodes)
        };
    }
}
