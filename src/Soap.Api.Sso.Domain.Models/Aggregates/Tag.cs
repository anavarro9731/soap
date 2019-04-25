namespace Soap.Api.Sso.Domain.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;

    public class Tag : Aggregate
    {
        public string NameOfTag { get; set; }
    }
}