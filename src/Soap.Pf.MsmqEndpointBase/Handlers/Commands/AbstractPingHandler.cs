namespace Soap.Pf.MsmqEndpointBase.Handlers.Commands
{
    public class AbstractPingHandlerForMsmq<TPing, TPongCommand, TPongEvent> : CommandHandler<TPing, TPongCommand>
        where TPongCommand : AbstractPongCommand, new()
        where TPongEvent : AbstractPongEvent, new()
        where TPing : AbstractPingCommandForMsmq<TPongCommand>, new()
    {
        protected override Task<TPongCommand> Handle(TPing message, ApiMessageMeta meta)
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

            return Task.FromResult(pong2);
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