namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;

    public class SeedDatabaseHandler : MessageHandler<ForwardCommandToQueue<SeedDatabase>>
    {
        protected override Task Handle(ForwardCommandToQueue<SeedDatabase> message, ApiMessageMeta meta)
        {
            UnitOfWork.SendCommand(message.Command);

            return Task.CompletedTask;
        }
    }
}