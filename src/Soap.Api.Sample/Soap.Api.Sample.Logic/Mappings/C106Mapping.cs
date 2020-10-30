//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public class C106Mapping : IMessageFunctionsClientSide<C106LargeCommand>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public IContinueProcess<C106LargeCommand>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C106LargeCommand msg)
        {
            Guard.Against(string.IsNullOrEmpty(msg.Large256KbString), "Should have been restored from blob storage but wasn't");
            return Task.CompletedTask;
        }

        public Task HandleFinalFailure(MessageFailedAllRetries<C106LargeCommand> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C106LargeCommand msg)
        {
        }
    }
}