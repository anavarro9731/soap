namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    /// <summary>
    ///     ##REMOVE-IN-COPY##
    ///     This is not a normal process and uses a number of undocumented features
    /// </summary>
    public class P201TestUnitOfWork : Process, IBeginProcess<C104v1_TestUnitOfWork>
    {
        public Func<C104v1_TestUnitOfWork, Task> BeginProcess =>
            async message =>
                {
                if (ContextWithMessageLogEntry.Current.Message.Headers.GetMessageId()
                    == SpecialIds.ConsideredAsRolledBackWhenFirstItemFails)
                {
                    await this.Get<UserOperations>().Call(x => x.ChangeLukeSkywalkersName)();
                }

                //* create x 2
                await this.Get<UserOperations>().Call(x => x.AddBobaAndLando)();

                if (ContextWithMessageLogEntry.Current.Message.Headers.GetMessageId()
                    == SpecialIds.FailsEarlyInReplayThenCompletesRemainderOfUow)
                {
                    await this.Get<UserOperations>().Call(x => x.ChangeLukeSkywalkersName)();
                    await this.Get<UserOperations>().Call(x => x.AddR2D2AndC3PO)();
                }

                //* delete x 2
                await this.Get<UserOperations>().Call(x => x.ArchivePrincessLeia)();
                await this.Get<UserOperations>().Call(x => x.DeleteDarthVader)();

                //* update 2 
                await this.Get<UserOperations>().Call(x => x.ChangeHansSoloName)(message.C104_HansSoloNewName);

                await this.Get<UserOperations>().Call(x => x.ChangeLukeSkywalkersName)();
                
                //* publish 1 event
                await PublishE150();
                //* send 1 command
                await SendC100();

                async Task PublishE150()
                {
                    
                    await Publish(
                        new E100v1_Pong
                        {
                            C000_PongedBy = nameof(P201TestUnitOfWork)
                        });
                }

                async Task SendC100()
                {
                    await Send(
                        new C100v1_Ping
                        {
                            C000_PingedBy = nameof(P201TestUnitOfWork)
                        });
                }
                };
    }
}
