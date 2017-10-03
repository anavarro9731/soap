namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class SeedDatabaseHandler : MessageHandler<SeedDatabase>
    {
        private readonly IProcess<SeedDatabaseProcess> process;

        public SeedDatabaseHandler(IProcess<SeedDatabaseProcess> process)
        {
            this.process = process;
        }

        protected override async Task Handle(SeedDatabase message, ApiMessageMeta meta)
        {
            await this.process.BeginProcess(message, meta);
        }
    }
}
