namespace Palmtree.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Messages.Events;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.Utility.PureFunctions;

    public class PingHandler : MessageHandler<PingCommand, PongViewModel>
    {
        public PongViewModel ProcessPing(PingCommand ping, ApiMessageMeta meta)
        {
            new PingCommandValidator().ValidateAndThrow(ping);

            switch (ping.PingedBy)
            {
                case "SpecialInvalidParamSeeCodeInHandler": //fail first time only
                    Guard.Against(meta.MessageLogItem.FailedAttempts.Count == 0, "Paramater Invalid");
                    break;

                case "OtherSpecialInvalidParamSeeCodeInHandler": //always fail
                    Guard.Against(true, "Paramater Invalid");
                    break;
            }

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

        protected override Task<PongViewModel> Handle(PingCommand message, ApiMessageMeta meta)
        {
            return Task.FromResult(ProcessPing(message, meta));
        }
    }
}
