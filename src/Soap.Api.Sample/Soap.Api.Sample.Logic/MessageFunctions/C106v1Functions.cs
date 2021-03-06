
namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;

    public class C106v1Functions : IMessageFunctionsClientSide<C106v1_LargeCommand>
    {
        public IContinueProcess<C106v1_LargeCommand>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C106v1_LargeCommand msg)
        {
            Guard.Against(string.IsNullOrEmpty(msg.C106_Large256KbString), "Should have been restored from blob storage but wasn't");
            return Task.CompletedTask;
        }

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C106v1_LargeCommand msg)
        {
            msg.Validate();
        }
    }
}
