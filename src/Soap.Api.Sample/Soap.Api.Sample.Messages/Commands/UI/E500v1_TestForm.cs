namespace Soap.Api.Sample.Messages.Commands.UI
{
    using Soap.Interfaces.Messages;
    using Soap.Pf.MessageContractsBase;

    public class E500v1_TestForm : UIFormDataEvent
    {
        protected override ApiCommand UserDefinedValues() => new C107v1_SampleDataTypes()
        {
            C107_String = "test default"
        };
    }
}
