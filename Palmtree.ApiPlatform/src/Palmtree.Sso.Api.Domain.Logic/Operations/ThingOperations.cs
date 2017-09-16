namespace Palmtree.Sample.Api.Domain.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class ThingOperations : Operations<Thing>
    {
        public async Task<Thing> AddThing(CreateThing msg)
        {
            {
                DetermineChange(out Thing thing);

                return await DataStore.Create(thing);
            }

            void DetermineChange(out Thing thing)
            {
                thing =  Thing.Create(msg.NameOfThing);
                thing.id = msg.ThingId;                
            }
        }

        public async Task<Thing> RemoveThing(DeleteThing msg)
        {
            var result = await DataStore.DeleteSoftById(msg.ThingId);
            return result;
        }

        public async Task<Thing> UpdateName(UpdateNameOfThing msg)
        {
            {
                DetermineChange(out Action<Thing> change, msg.NameOfThing);

                var result = await DataStore.UpdateById(msg.ThingId, change);

                return result;
            }

            void DetermineChange(out Action<Thing> change, string nameOfThing)
            {
                change = t => t.NameOfThing = nameOfThing;
            }
        }

    }
}
