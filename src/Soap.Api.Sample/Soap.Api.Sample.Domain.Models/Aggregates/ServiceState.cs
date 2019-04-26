namespace Soap.Api.Sample.Domain.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Utility;

    public class ServiceState : Aggregate
    {
        public FlaggedState DatabaseState { get; set; }
    }
}