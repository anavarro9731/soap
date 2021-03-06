namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility;

    public class P209_C107__CreateOrUpdateTestData : Process, IBeginProcess<C107v1_CreateOrUpdateTestDataTypes>
    {
        public Func<C107v1_CreateOrUpdateTestDataTypes, Task> BeginProcess =>
            async msg =>
                {

                {
                    await CreateOrUpdateTestData();

                    await PublishTestDataAdded();
                }

                async Task PublishTestDataAdded()
                {
                    await Bus.Publish(new E104v1_TestDataUpserted(msg.C107_Guid.Value));
                }

                async Task CreateOrUpdateTestData()
                {
                    Guard.Against(msg.C107_Long == 9731, "Force Test Error");
                    
                    await this.Get<TestDataOperations>().Call(x => x.SetTestData)(msg);
                }
                };
    }
}
