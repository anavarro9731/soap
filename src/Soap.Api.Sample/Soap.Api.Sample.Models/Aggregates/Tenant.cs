namespace Soap.Api.Sample.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeys;

    [PartitionKey__Shared]
    [BypassSecurity(SecurableOperations.READ)]
    public class Tenant : Aggregate
    {
        public string Name { get; set; }
    }
}