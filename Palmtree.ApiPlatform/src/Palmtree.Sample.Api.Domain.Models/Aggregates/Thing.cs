namespace Palmtree.Sample.Api.Domain.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;

    public class Thing : Aggregate
    {
        public string NameOfThing { get; set; }
    }
}
