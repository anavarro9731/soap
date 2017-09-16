namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Logic.Processes;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

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
