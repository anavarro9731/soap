﻿namespace Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure
{
    using System.Threading.Tasks;
    using Rebus.Handlers;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Interfaces;

    public class CommandHandler : IHandleMessages<IApiCommand>
    {
        private readonly IMessagePipeline messagePipeline;

        public CommandHandler(IMessagePipeline messagePipeline)
        {
            this.messagePipeline = messagePipeline;
        }

        public async Task Handle(IApiCommand command)
        {
            await this.messagePipeline.Execute(command).ConfigureAwait(false);
        }
    }
}
