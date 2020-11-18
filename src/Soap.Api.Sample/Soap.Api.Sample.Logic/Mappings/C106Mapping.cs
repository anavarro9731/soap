//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Operations;

    public class C106Mapping : IMessageFunctionsClientSide<C106v1LargeCommand>
    {
        public IContinueProcess<C106v1LargeCommand>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C106v1LargeCommand msg)
        {
            Guard.Against(string.IsNullOrEmpty(msg.C106_Large256KbString), "Should have been restored from blob storage but wasn't");
            return Task.CompletedTask;
        }

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C106v1LargeCommand msg)
        {
        }
    }
}