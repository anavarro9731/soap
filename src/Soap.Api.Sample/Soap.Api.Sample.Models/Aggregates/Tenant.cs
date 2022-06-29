namespace Soap.Api.Sample.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;

    [BypassSecurity(SecurableOperations.READ)]
    public class Tenant : Aggregate
    {
        public string Name { get; set; }
    }
}