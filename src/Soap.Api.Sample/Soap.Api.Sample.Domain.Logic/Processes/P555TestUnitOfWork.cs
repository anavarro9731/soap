namespace Sample.Logic.Processes
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using Sample.Logic.Operations;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.ProcessesAndOperations;

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

                async Task SimulateAnotherUnitOfWorkChangingLukesRecord()
                {
                    var store = new DataStore(ContextWithMessageLogEntry.Current.DataStore.DocumentRepository);
                    var luke = (await store.ReadActive<User>(l => l.UserName == "luke.skywalker")).Single();

                    await store.UpdateById<User>(
                        luke.id,
                        luke => luke.Roles.Add(new Role { Name = "doesnt matter just make a change to add a history item" }));
                    await store.CommitChanges();
                }
                };
    }
}