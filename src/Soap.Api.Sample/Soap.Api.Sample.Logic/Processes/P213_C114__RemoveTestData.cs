namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P213_C114__RemoveTestData : Process, IBeginProcess<C114v1_DeleteTestDataById>
    {
        public Func<C114v1_DeleteTestDataById, Task> BeginProcess =>
            async msg =>
                {
                {
                    await DeleteTestData();

                    await PublishTestDataAdded();
                }

                async Task PublishTestDataAdded()
                {
                    await Bus.Publish(new E106v1_TestDataRemoved(msg.C114_TestDataId.Value));
                }

                async Task DeleteTestData()
                {
                    await this.Get<TestDataOperations>().Call(x => x.DeleteTestDataById)(msg);
                }
                };
    }
}