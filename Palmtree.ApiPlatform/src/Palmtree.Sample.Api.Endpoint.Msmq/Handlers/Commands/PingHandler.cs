namespace Palmtree.Sample.Api.Endpoint.Msmq.Handlers.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Utility.PureFunctions;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Messages.Events;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;

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
