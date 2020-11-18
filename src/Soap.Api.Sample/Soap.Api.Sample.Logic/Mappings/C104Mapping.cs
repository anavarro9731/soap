//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C104Mapping : IMessageFunctionsClientSide<C104v1TestUnitOfWork>
    {

        public IContinueProcess<C104v1TestUnitOfWork>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C104v1TestUnitOfWork msg) => this.Get<P555TestUnitOfWork>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C104v1TestUnitOfWork msg) => new C104v1TestUnitOfWork.C104Validator().ValidateAndThrow(msg);
    }
}