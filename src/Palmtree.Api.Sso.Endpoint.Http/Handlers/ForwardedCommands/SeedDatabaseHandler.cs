namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.ClientServerMessaging.Commands;

    public class SeedDatabaseHandler : MessageHandler<ForwardCommandFromHttpToMsmq<SeedDatabase>>
    {
        protected override Task Handle(ForwardCommandFromHttpToMsmq<SeedDatabase> message, ApiMessageMeta meta)
        {
            UnitOfWork.SendCommand(message.Command);

            return Task.CompletedTask;
        }
    }
}