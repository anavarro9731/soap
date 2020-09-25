//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C104Mapping : IMessageFunctionsClientSide<C104TestUnitOfWork>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public IContinueProcess<C104TestUnitOfWork>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C104TestUnitOfWork msg) => this.Get<P555TestUnitOfWork>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C104TestUnitOfWork> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C104TestUnitOfWork msg) => new C104TestUnitOfWork.C104Validator().ValidateAndThrow(msg);
    }
}