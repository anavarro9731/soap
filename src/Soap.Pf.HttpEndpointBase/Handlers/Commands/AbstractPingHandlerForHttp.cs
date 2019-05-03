namespace Soap.Pf.HttpEndpointBase.Handlers.Commands
{
    using System.Threading.Tasks;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.MessageContractsBase.Commands;

    public class AbstractPingHandlerForHttp<TPing, TPongResponseViewModel, TPongCommand> : CommandHandler<TPing, TPongResponseViewModel>
        where TPing : AbstractPingCommandForHttp<TPongResponseViewModel>, new()
        where TPongResponseViewModel : AbstractPingCommandForHttp<TPongResponseViewModel>.AbstractResponseModel, new()
        where TPongCommand : AbstractPongCommand, new()
    {
        protected override Task<TPongResponseViewModel> Handle(TPing message, ApiMessageMeta meta)
        {
            AssertTestScenarios(message, meta);

            var pong2 = new TPongCommand
            {
                PingedAt = message.PingedAt, PingedBy = message.PingedBy
            };

            UnitOfWork.SendCommand(pong2);

            var pong3 = new TPongResponseViewModel
            {
                PingedAt = message.PingedAt, PingedBy = message.PingedBy
            };

            return Task.FromResult(pong3);
        }

        private static void AssertTestScenarios(TPing ping, ApiMessageMeta meta)
        {
            switch (ping.PingedBy)
            {
                case "SpecialInvalidParamSeeCodeInHandler": //fail first time only
                    Guard.Against(meta.MessageLogItem.FailedAttempts.Count == 0, "Parameter Invalid");
                    break;
            }
        }
    }
}