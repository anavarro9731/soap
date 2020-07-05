namespace Sample.Logic.Mappers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C102Mapping : IMessageFunctionsClientSide<C102GetServiceState>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper() => new Dictionary<ErrorCode, ErrorCode>();

        public Task Handle(C102GetServiceState msg)
        {
            return this.Get<GetServiceStateProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries<C102GetServiceState> msg)
        {
            return this.Get<NotifyOfFinalFailureProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public void Validate(C102GetServiceState msg)
        {
            new C102GetServiceState.Validator().ValidateAndThrow(msg);
        }
    }
}