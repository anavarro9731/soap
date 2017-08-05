namespace Palmtree.Sample.Api.Domain.Logic.Processes
{
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Palmtree.ApiPlatform.Infrastructure.Messages.Generic;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class UserAddsThingProcess : Process<UserAddsThingProcess>, IBeginProcess<CreateAggregate<Thing>, Thing>
    {
        private readonly ThingOperations thingOperations;

        private readonly UserOperations userOperations;

        public UserAddsThingProcess(ThingOperations thingOperations, UserOperations userOperations)
        {
            this.thingOperations = thingOperations;
            this.userOperations = userOperations;
        }

        public async Task<Thing> BeginProcess(CreateAggregate<Thing> message, ApiMessageMeta meta)
        {
            {
                var newThing = await AddThingToDatabase();

                await AddThingToUser(newThing, meta.RequestedBy);

                return newThing;
            }

            async Task<Thing> AddThingToDatabase()
            {
                return await this.thingOperations.AddThing(message);
            }

            async Task AddThingToUser(Thing thing, IUserWithPermissions requestedBy)
            {
                await this.userOperations.AddThingToUser(thing, requestedBy.id);
            }
        }
    }
}
