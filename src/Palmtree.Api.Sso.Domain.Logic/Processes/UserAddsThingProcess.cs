namespace Palmtree.Api.Sso.Domain.Logic.Processes
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class UserAddsThingProcess : Process<UserAddsThingProcess>, IBeginProcess<CreateThing, Thing>
    {
        private readonly ThingOperations thingOperations;

        private readonly UserOperations userOperations;

        public UserAddsThingProcess(ThingOperations thingOperations, UserOperations userOperations)
        {
            this.thingOperations = thingOperations;
            this.userOperations = userOperations;
        }

        public async Task<Thing> BeginProcess(CreateThing message, ApiMessageMeta meta)
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
