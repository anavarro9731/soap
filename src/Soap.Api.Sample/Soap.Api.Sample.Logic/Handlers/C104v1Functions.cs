//##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C104v1Functions : IMessageFunctionsClientSide<C104v1_TestUnitOfWork>
    {

        public IContinueProcess<C104v1_TestUnitOfWork>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C104v1_TestUnitOfWork msg) => this.Get<P201TestUnitOfWork>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C104v1_TestUnitOfWork msg) => msg.Validate();
    }
}
