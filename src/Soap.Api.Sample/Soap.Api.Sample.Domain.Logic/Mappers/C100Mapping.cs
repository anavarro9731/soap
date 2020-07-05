namespace Sample.Logic.Mappers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C100Mapping : IMessageFunctionsClientSide<C100Ping>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper() => new Dictionary<ErrorCode, ErrorCode>();

        public Task Handle(C100Ping msg)
        {
            return this.Get<PingPongProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries<C100Ping> msg)
        {
            return this.Get<NotifyOfFinalFailureProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public void Validate(C100Ping msg)
        {
        }
    }
}