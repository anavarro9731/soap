namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Logic.Operations;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.NotificationServer;

    /// <summary>
    ///     $$REMOVE-IN-COPY$$
    ///     This is not a normal process and uses a number of undocumented features
    /// </summary>
    public class P555TestUnitOfWork : Process, IBeginProcess<C104TestUnitOfWork>
    {
        public Func<C104TestUnitOfWork, Task> BeginProcess =>
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
                await this.Get<UserOperations>().Call(x => x.ChangeHansSoloName)(message.HansSoloNewName);

                await this.Get<UserOperations>().Call(x => x.ChangeLukeSkywalkersName)();
                
                //* publish 1 event
                await PublishE150();
                //* send 1 command
                await SendC100();

                async Task PublishE150()
                {
                    await Bus.Publish(
                        new E150Pong
                        {
                            PongedBy = nameof(P555TestUnitOfWork)
                        });
                }

                async Task SendC100()
                {
                    await Bus.Send(
                        new C100Ping
                        {
                            PingedBy = nameof(P555TestUnitOfWork)
                        });
                }
                };
    }
}