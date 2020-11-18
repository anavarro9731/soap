﻿//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C105Mapping : IMessageFunctionsClientSide<C105v1SendLargeMessage>
    {

        public IContinueProcess<C105v1SendLargeMessage>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C105v1SendLargeMessage msg) => this.Get<P560SendLargeMessage>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C105v1SendLargeMessage msg)
        {
        }
    }
}