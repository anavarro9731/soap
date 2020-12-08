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

        protected override ApiCommand UserDefinedValues()
        {
            var c107PostCodesSingle = new EnumerationAndFlags(this.postCodes.First(), this.postCodes, allowMultipleSelections:false);
            var c107PostCodesSingleOptional = new EnumerationAndFlags(allEnumerations:this.postCodes, allowMultipleSelections:false);
            return new C107v1_TestDataTypes()
            {
                C107_String = "test default",
                C107_PostCodesSingle = c107PostCodesSingle,
                C107_PostCodesSingleOptional = c107PostCodesSingleOptional,
                C107_PostCodesMulti = new EnumerationAndFlags(this.postCodes.First(), this.postCodes),
                C107_PostCodesMultiOptional = new EnumerationAndFlags(allEnumerations:this.postCodes)
            };
        }
    }
}
