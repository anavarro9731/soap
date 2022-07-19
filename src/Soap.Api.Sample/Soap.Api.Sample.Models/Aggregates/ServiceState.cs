namespace Soap.Api.Sample.Models.Aggregates
{
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeys;

    [PartitionKey__Shared]
    public class ServiceState : Aggregate
    {
        public EnumerationFlags DatabaseState { get; set; }
    }
}
