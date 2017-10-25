namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;

    public class SeedDatabaseHandler : MessageHandler<ForwardCommandToQueue<SeedDatabase>>
    {
        protected override Task Handle(ForwardCommandToQueue<SeedDatabase> message, ApiMessageMeta meta)
        {
            UnitOfWork.SendCommand(message.Command);

            return Task.CompletedTask;
        }
    }
}