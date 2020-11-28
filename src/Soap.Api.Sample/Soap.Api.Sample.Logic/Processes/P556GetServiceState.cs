namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P556GetServiceState : Process, IBeginProcess<C102v1_GetServiceState>
    {
        public Func<C102v1_GetServiceState, Task> BeginProcess =>
            async message =>
                {
                var serviceState = await this.Get<ServiceStateQueries>().Call(x => x.GetServiceState)();

                var gotServiceState = new E151v1_GotServiceState
                {
                    E151_State = new E151v1_GotServiceState.ServiceState
                    {
                        E151_DatabaseState = serviceState.DatabaseState
                    }
                };

                await Publish(gotServiceState);
                };
    }
}