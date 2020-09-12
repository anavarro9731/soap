namespace Soap.Api.Sample.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;
    using Soap.Utility.Objects.Binary;

    public class ServiceState : Aggregate
    {
        public Flags DatabaseState { get; set; }
    }
}