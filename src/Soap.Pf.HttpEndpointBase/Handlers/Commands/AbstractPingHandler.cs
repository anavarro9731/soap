namespace Soap.Pf.HttpEndpointBase.Handlers.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.HttpEndpointBase;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Pf.MessageContractsBase.Events;

    public class AbstractPingHandler<TPing, TPongResponseViewModel, TPongCommand, TPongEvent> : CommandHandler<TPing, TPongResponseViewModel>
        where TPing : AbstractPingCommand<TPongResponseViewModel>, new()
        where TPongResponseViewModel : AbstractPingCommand<TPongResponseViewModel>.AbstractResponseModel, new()
        where TPongCommand : AbstractPongCommand, new()
        where TPongEvent : AbstractPongEvent, new()
    {
        protected override Task<TPongResponseViewModel> Handle(TPing message, ApiMessageMeta meta)
        {
            AssertTestScenarios(message, meta);

            var pong = new TPongEvent
            {
                PingedAt = message.PingedAt, PingedBy = message.PingedBy, OccurredAt = DateTime.UtcNow
            };
            UnitOfWork.PublishEvent(pong);

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
                    Guard.Against(meta.MessageLogItem.FailedAttempts.Count == 0, "Paramater Invalid");
                    break;

                case "OtherSpecialInvalidParamSeeCodeInHandler": //always fail
                    Guard.Against(true, "Paramater Invalid");
                    break;
            }
        }
    }
}