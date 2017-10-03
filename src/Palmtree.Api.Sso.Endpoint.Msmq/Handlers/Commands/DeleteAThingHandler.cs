namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;

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
