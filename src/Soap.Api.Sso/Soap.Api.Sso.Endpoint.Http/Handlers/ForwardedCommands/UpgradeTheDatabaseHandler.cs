namespace Soap.Api.Sso.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.HttpEndpointBase;

    public class UpgradeTheDatabaseHandler : CommandHandler<ForwardCommandFromHttpToMsmq<UpgradeTheDatabaseCommand>>
    {
        protected override Task Handle(ForwardCommandFromHttpToMsmq<UpgradeTheDatabaseCommand> message, ApiMessageMeta meta)
        {
            (message.CommandToForward as UpgradeTheDatabaseCommand).EnvelopeId = message.MessageId;

            UnitOfWork.SendCommand(message.CommandToForward);

            return Task.CompletedTask;
        }
    }
}