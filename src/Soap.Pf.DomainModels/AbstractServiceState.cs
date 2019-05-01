namespace Soap.Pf.DomainModelsBase
{
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Utility;

    public class AbstractServiceState : Aggregate
    {
        public FlaggedState DatabaseState { get; set; }
    }
}