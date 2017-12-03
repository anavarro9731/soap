namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;

    public class UpdateNameOfThingHandler : MessageHandler<UpdateNameOfThing, Thing>
    {
        private readonly ThingOperations thingOperations;

        public UpdateNameOfThingHandler(ThingOperations thingOperations)
        {
            this.thingOperations = thingOperations;
        }

        protected override async Task<Thing> Handle(UpdateNameOfThing message, ApiMessageMeta meta)
        {
            return await this.thingOperations.UpdateName(message);
        }
    }
}