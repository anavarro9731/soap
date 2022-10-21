
    namespace Soap.Api.Sample.Logic.MessageFunctions
    {
        using System.Threading.Tasks;
        using Soap.Api.Sample.Logic.Processes;
        using Soap.Api.Sample.Messages.Commands;
        using Soap.Interfaces;
        using Soap.Interfaces.Messages;
    
        public class C116v1Functions : IMessageFunctionsClientSide<C116v1_TruncateMessageLog>
        {
            public IContinueProcess<C116v1_TruncateMessageLog>[] HandleWithTheseStatefulProcesses { get; }
    
            public Task Handle(C116v1_TruncateMessageLog msg) => this.Get<P216_C116__HandleTruncateMessageLog>().Call(x => x.BeginProcess)(msg);
    
            public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
                this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);
    
            public void Validate(C116v1_TruncateMessageLog msg)
            {
                msg.Validate();
            }
        }
    }
