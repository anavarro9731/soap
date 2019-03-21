namespace Palmtree.Api.Sso.Endpoint.Http
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Messages.Events;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.HttpEndpointBase;

    public class PingHandler : CommandHandler<PingCommand, PingCommand.PongViewModel>
    {
        public PingCommand.PongViewModel ProcessPing(PingCommand ping, ApiMessageMeta meta)
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

            var pong3 = new PingCommand.PongViewModel
            {
                PingedAt = ping.PingedAt,
                PingedBy = ping.PingedBy
            };
            return pong3;
        }

        protected override Task<PingCommand.PongViewModel> Handle(PingCommand message, ApiMessageMeta meta)
        {
            return Task.FromResult(ProcessPing(message, meta));
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
    }
}