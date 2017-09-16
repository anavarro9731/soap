namespace Palmtree.Sample.Api.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Messages.Commands;

    public class SeedDatabaseHandler : MessageHandler<ForwardMessageToQueue<SeedDatabase>>
    {
        protected override Task Handle(ForwardMessageToQueue<SeedDatabase> message, ApiMessageMeta meta)
        {
            UnitOfWork.SendCommand(message.Message);

            return Task.CompletedTask;
        }
    }
}
