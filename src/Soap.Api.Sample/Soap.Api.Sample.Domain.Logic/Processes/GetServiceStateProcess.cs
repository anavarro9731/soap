namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Logic.Queries;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class GetServiceStateProcess : Process, IBeginProcess<C102GetServiceState>
    {
        public Func<C102GetServiceState, Task> BeginProcess =>
            async message =>
                {
                var serviceState = await this.Get<ServiceStateQueries>().Exec(x => x.GetServiceState)();

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