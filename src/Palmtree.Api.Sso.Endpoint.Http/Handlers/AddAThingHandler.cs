namespace Palmtree.Api.Sso.Endpoint.Http.Handlers
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.HttpEndpointBase;

    public class AddAThingHandler : CommandHandler<CreateThing, Thing>
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