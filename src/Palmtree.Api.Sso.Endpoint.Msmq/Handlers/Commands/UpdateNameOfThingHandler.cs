namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Operations;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.MsmqEndpointBase;

    public class UpdateNameOfThingHandler : CommandHandler<UpdateNameOfThing>
    {
        private readonly ThingOperations thingOperations;

        public UpdateNameOfThingHandler(ThingOperations thingOperations)
        {
            this.thingOperations = thingOperations;
        }

        protected override async Task Handle(UpdateNameOfThing message, ApiMessageMeta meta)
        {
            await this.thingOperations.UpdateName(message);
        }
    }
}