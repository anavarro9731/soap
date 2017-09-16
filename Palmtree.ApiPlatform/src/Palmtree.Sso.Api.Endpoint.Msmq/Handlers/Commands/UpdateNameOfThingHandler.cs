namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Logic.Operations;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

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
