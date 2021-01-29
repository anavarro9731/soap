//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P202GetServiceState : Process, IBeginProcess<C102v1_GetServiceState>
    {
        public Func<C102v1_GetServiceState, Task> BeginProcess =>
            async message =>
                {
                {
                    var serviceState = await GetServiceState();

                    await PublishE101v1(serviceState);
                }

                async Task<ServiceState> GetServiceState()
                {
                    return await this.Get<ServiceStateQueries>().Call(x => x.GetServiceStateById)();
                }

                async Task PublishE101v1(ServiceState serviceState)
                {
                    var gotServiceState = new E101v1_GotServiceState
                    {
                        E101_State = new E101v1_GotServiceState.ServiceState
                        {
                            E101_DatabaseState = serviceState.DatabaseState
                        }
                    };

                    await Bus.Publish(gotServiceState);
                }
                };
    }
}
