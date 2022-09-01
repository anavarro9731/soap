
    namespace Soap.Api.Sample.Logic.MessageFunctions
    {
        using System.Threading.Tasks;
        using Soap.Api.Sample.Logic.Processes;
        using Soap.Api.Sample.Messages.Commands;
        using Soap.Interfaces;
        using Soap.Interfaces.Messages;
    
        public class C108v1Functions : IMessageFunctionsClientSide<C108v1_TestEventPublishedToQueue>
        {
            public IContinueProcess<C108v1_TestEventPublishedToQueue>[] HandleWithTheseStatefulProcesses { get; }
    
            public Task Handle(C108v1_TestEventPublishedToQueue msg) => this.Get<P215_C108__HandleTestEventPublishedToQueue>().Call(x => x.BeginProcess)(msg);
    
            public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
                this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);
    
            public void Validate(C108v1_TestEventPublishedToQueue msg)
            {
                msg.Validate();
            }
        }
    }
