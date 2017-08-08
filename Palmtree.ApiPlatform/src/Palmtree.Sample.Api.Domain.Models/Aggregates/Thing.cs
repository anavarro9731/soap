namespace Palmtree.Sample.Api.Domain.Models.Aggregates
{
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;

    public class Thing : Aggregate
    {
        public string NameOfThing { get; set; }

        public static Thing Create(string nameOfThing)
        {
            return new Thing()
            {
                NameOfThing = nameOfThing
            };
        }
    }
}
