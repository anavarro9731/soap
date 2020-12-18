namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C107v1Functions : IMessageFunctionsClientSide<C107v1_CreateOrUpdateTestDataTypes>
    {
        public IContinueProcess<C107v1_CreateOrUpdateTestDataTypes>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C107v1_CreateOrUpdateTestDataTypes msg) =>
            this.Get<P209CreateOrUpdateTestData>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C107v1_CreateOrUpdateTestDataTypes msg)
        {
            msg.Validate();
        }


    }
}
