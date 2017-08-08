namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Processes;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class AddAThingHandler : MessageHandler<CreateThing, Thing>
    {
        private readonly IProcess<UserAddsThingProcess> process;

        public AddAThingHandler(IProcess<UserAddsThingProcess> process)
        {
            this.process = process;
        }

        protected override async Task<Thing> Handle(CreateThing message, ApiMessageMeta meta)
        {
            return await this.process.BeginProcess<CreateThing, Thing>(message, meta);
        }
    }
}
