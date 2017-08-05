namespace Palmtree.Sample.Api.Domain.Logic.Operations
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure.Messages.Generic;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class ThingOperations : Operations<Thing>
    {
        public async Task<Thing> AddThing(CreateAggregate<Thing> msg)
        {
            return await DataStore.Create(msg.Model);
        }

        public async Task<Thing> RemoveThing(DeleteAggregate<Thing> msg)
        {
            var result = await DataStore.DeleteSoftById(msg.IdToDelete);
            return result;
        }

        public async Task<Thing> UpdateThing(UpdateAggregate<Thing> msg)
        {
            return await DataStore.Update(msg.UpdatedModel);
        }
    }
}
