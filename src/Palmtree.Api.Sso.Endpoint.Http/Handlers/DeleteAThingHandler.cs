namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class DeleteAThingHandler : CommandHandler<DeleteThing, Thing>
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