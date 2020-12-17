namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Commands.UI;
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
            var c107PostCodesMultiOptional = new EnumerationAndFlags(allEnumerations: this.postCodes);
            c107PostCodesMultiOptional.AddFlag(this.postCodes.First());
            c107PostCodesMultiOptional.AddFlag(this.postCodes.Last());
            
            return new C107v1_CreateOrUpdateTestDataTypes()
            {
                C107_Decimal = 0,
                C107_Guid = Guid.NewGuid(),
                C107_DateTime = DateTime.UtcNow,
                C107_Long = 0,
                C107_String = "test default",
                C107_PostCodesSingle = new EnumerationAndFlags(this.postCodes.First(), this.postCodes, false),
                C107_PostCodesSingleOptional = new EnumerationAndFlags(this.postCodes.Last(), this.postCodes, false),
                C107_PostCodesMulti = new EnumerationAndFlags(this.postCodes.First(), this.postCodes),
                C107_PostCodesMultiOptional = c107PostCodesMultiOptional,
                C107_CustomObject = new C107v1_CreateOrUpdateTestDataTypes.Address()
                {
                    C107_Town = "pontrilas"
                },
                // C107_File = new BlobMeta
                // {
                //     Name = "soap.zip",
                //     Id = Guid.NewGuid()
                // },
                // C107_Image = new BlobMeta
                // {
                //     Name = "soap.png",
                //     Id = Guid.NewGuid()
                // }, 
            };
        }

        public override void Validate()
        {
            
        }
    }
}
