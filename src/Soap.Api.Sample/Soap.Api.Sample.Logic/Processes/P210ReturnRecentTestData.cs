namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P210ReturnRecentTestData : Process, IBeginProcess<C111v2_GetRecentTestData>
    {
        public Func<C111v2_GetRecentTestData, Task> BeginProcess =>
            async msg =>
                {
                var recentTestData = await this.Get<TestDataQueries>()
                                               .Call(x => x.GetRecentTestData(msg.MaxAgeInDays ?? 5, msg.MaxRecords ?? 10))();

                var response = new E105v1_GotRecentTestData
                {
                    E105_TestData = new List<E105v1_GotRecentTestData.TestData>(
                        recentTestData.Select(
                            e => new E105v1_GotRecentTestData.TestData
                            {
                                E105_Guid = e.Guid,
                                E105_Id = e.id,
                                E105_Label = e.String,
                                E105_CreatedAt = e.Created
                            }))
                };

                await Bus.Publish(
                    response,
                    new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
                };
    }
}
