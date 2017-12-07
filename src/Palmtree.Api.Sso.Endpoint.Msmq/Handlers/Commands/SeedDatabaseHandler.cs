﻿namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Logic.Processes;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.MsmqEndpointBase;

    public class SeedDatabaseHandler : CommandHandler<SeedDatabase>
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