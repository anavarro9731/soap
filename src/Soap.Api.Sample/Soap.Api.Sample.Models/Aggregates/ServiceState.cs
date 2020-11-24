namespace Soap.Api.Sample.Models.Aggregates
{
    using CircuitBoard;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Binary;

    public class ServiceState : Aggregate
    {
        public EnumerationFlags DatabaseState { get; set; }
    }
}
