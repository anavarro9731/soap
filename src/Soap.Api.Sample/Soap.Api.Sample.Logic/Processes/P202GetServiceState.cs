//##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P202GetServiceState : Process, IBeginProcess<C102v1_GetServiceState>
    {
        public Func<C102v1_GetServiceState, Task> BeginProcess =>
            async message =>
                {
                var serviceState = await this.Get<ServiceStateQueries>().Call(x => x.GetServiceState)();

                var gotServiceState = new E101v1_GotServiceState
                {
                    E101_State = new E101v1_GotServiceState.ServiceState
                    {
                        E101_DatabaseState = serviceState.DatabaseState
                    }
                };

                await Publish(gotServiceState);
                };
    }
}
