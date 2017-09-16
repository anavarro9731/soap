namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class DeleteAThingHandler : MessageHandler<DeleteThing, Thing>
    {
        private readonly ThingOperations thingOperations;

        public DeleteAThingHandler(ThingOperations thingOperations)
        {
            this.thingOperations = thingOperations;
        }

        protected override async Task<Thing> Handle(DeleteThing message, ApiMessageMeta meta)
        {
            return await this.thingOperations.RemoveThing(message);
        }
    }
}
