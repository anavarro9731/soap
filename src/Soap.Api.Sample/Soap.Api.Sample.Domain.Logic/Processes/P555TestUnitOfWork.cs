namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Logic.Operations;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class P555TestUnitOfWork : Process, IBeginProcess<C104TestUnitOfWork>
    {
        public Func<C104TestUnitOfWork, Task> BeginProcess =>
            async message =>
                {
                await this.Get<UserOperations>().Call(x => x.ChangeHansSoloName)(message.HansSoloNewName);
                await this.Get<UserOperations>().Call(x => x.AddTestUsers)();
                await this.Get<UserOperations>().Call(x => x.RemoveDarthVader)();
                await PublishE150();
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