namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Logic.Queries;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class P100GetServiceState : Process, IBeginProcess<C102GetServiceState>
    {
        public Func<C102GetServiceState, Task> BeginProcess =>
            async message =>
                {
                     var serviceState = await this.Get<ServiceStateQueries>().Call(x => x.GetServiceState)();

                    var gotServiceState = new E151GotServiceState
                    {
                        State = new E151GotServiceState.ServiceState
                        {
                            DatabaseState = serviceState.DatabaseState
                        }
                    };

                    await Bus.Publish(gotServiceState);

                };
    }
}