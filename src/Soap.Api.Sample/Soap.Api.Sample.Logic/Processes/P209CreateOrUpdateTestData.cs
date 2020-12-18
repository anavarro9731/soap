namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P209CreateOrUpdateTestData : Process, IBeginProcess<C107v1_CreateOrUpdateTestDataTypes>
    {
        public Func<C107v1_CreateOrUpdateTestDataTypes, Task> BeginProcess =>
            async msg =>
                {
                await this.Get<TestDataOperations>().Call(x => x.SetTestData)(msg);

                await Publish(new E104v1_TestDataAdded(msg.C107_Guid.Value));
                };
    }
}
