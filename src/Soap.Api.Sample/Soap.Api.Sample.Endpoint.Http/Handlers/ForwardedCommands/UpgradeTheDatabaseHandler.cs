namespace Soap.Api.Sample.Endpoint.Http.Handlers.ForwardedCommands
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.HttpEndpointBase;

    public class UpgradeTheDatabaseHandler : CommandHandler<ForwardCommandFromHttpToMsmq<UpgradeTheDatabase>>
    {
        protected override Task Handle(ForwardCommandFromHttpToMsmq<UpgradeTheDatabase> message, ApiMessageMeta meta)
        {

            (message.CommandToForward as UpgradeTheDatabase).EnvelopeId = message.MessageId;

            UnitOfWork.SendCommand(message.CommandToForward);

            return Task.CompletedTask;
        }
    }
}