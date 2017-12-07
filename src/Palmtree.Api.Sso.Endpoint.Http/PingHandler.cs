namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Messages.Events;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.HttpEndpointBase;

    public class PingHandler : CommandHandler<PingCommand, PongViewModel>
    {
        public PongViewModel ProcessPing(PingCommand ping, ApiMessageMeta meta)
        {
            new PingCommandValidator().ValidateAndThrow(ping);

            AssertTestScenarios(ping, meta);

            var pong = new PongEvent
            {
                PingedAt = ping.PingedAt,
                PingedBy = ping.PingedBy,
                OccurredAt = DateTime.UtcNow
            };
            UnitOfWork.PublishEvent(pong);

            var pong2 = new PongCommand
            {
                PingedAt = ping.PingedAt,
                PingedBy = ping.PingedBy
            };

            UnitOfWork.SendCommand(pong2);

            var pong3 = new PongViewModel
            {
                PingedAt = ping.PingedAt,
                PingedBy = ping.PingedBy
            };
            return pong3;
        }

        private static void AssertTestScenarios(PingCommand ping, ApiMessageMeta meta)
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

        protected override Task<PongViewModel> Handle(PingCommand message, ApiMessageMeta meta)
        {
            return Task.FromResult(ProcessPing(message, meta));
        }
    }
}