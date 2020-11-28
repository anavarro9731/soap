namespace Soap.Api.Sample.Models.Aggregates
{
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel;

    public class ServiceState : Aggregate
    {
        public EnumerationFlags DatabaseState { get; set; }
    }
}
