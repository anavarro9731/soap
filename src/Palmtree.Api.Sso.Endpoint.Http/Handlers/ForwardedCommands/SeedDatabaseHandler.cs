namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;

    public class SeedDatabaseHandler : MessageHandler<ForwardMessageToQueue<SeedDatabase>>
    {
        protected override Task Handle(ForwardMessageToQueue<SeedDatabase> message, ApiMessageMeta meta)
        {
            UnitOfWork.SendCommand(message.Message);

            return Task.CompletedTask;
        }
    }
}
