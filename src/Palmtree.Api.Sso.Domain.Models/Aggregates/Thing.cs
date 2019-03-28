namespace Palmtree.Api.Sso.Domain.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;
    using DataStore.Providers.CosmosDb;

    public class Thing : CosmosAggregate
    {
        public string NameOfThing { get; set; }

        public static Thing Create(string nameOfThing)
        {
            return new Thing
            {
                NameOfThing = nameOfThing
            };
        }
    }
}