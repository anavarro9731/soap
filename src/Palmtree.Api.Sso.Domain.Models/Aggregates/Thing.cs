namespace Palmtree.Api.Sso.Domain.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;

    public class Thing : Aggregate
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