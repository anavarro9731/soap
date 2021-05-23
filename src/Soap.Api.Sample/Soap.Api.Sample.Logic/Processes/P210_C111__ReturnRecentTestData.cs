namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;

    public class P210_C111__ReturnRecentTestData : Process, IBeginProcess<C111v2_GetRecentTestData>
    {
        public Func<C111v2_GetRecentTestData, Task> BeginProcess =>
            async msg =>
                {
                {
                    var recentTestData = await GetRecentTestData();

                    await PublishGotRecentTestData(recentTestData);
                }

                async Task PublishGotRecentTestData(List<TestData> recentTestData)
                {
                    var response = new E105v1_GotRecentTestData
                    {
                        E105_TestData = new List<E105v1_GotRecentTestData.TestData>(
                            recentTestData.Select(
                                e => new E105v1_GotRecentTestData.TestData
                                {
                                    E105_Guid = e.Guid,
                                    E105_Id = e.id,
                                    E105_Label = e.String,
                                    E105_CreatedAt = e.Created,
                                    E105_CChild = e.CChild?.Map(c => new E105v1_GotRecentTestData.ChildC()
                                    {
                                        E105_BChildren = c.BChildren?.Select(b => new E105v1_GotRecentTestData.ChildB()
                                        {
                                            E105_BChildBool = b.Bool,
                                            E105_BChildId = b.id,
                                            E105_BChildLong = b.Long,
                                            E105_BChildString = b.String
                                        }).ToList() ?? new List<E105v1_GotRecentTestData.ChildB>(),
                                        E105_BChild = c.BChild?.Map(b => new E105v1_GotRecentTestData.ChildB()
                                        {
                                            E105_BChildId = b.id,
                                            E105_BChildBool = b.Bool,
                                            E105_BChildLong = b.Long,
                                            E105_BChildString = b.String
                                        }),
                                        E105_CChildId = c.id,
                                        E105_CChildString = c.String
                                    }),
                                    E105_CChildren = e.CChildren?.Select(c => new E105v1_GotRecentTestData.ChildC()
                                    {
                                        E105_BChildren = c.BChildren?.Select(b => new E105v1_GotRecentTestData.ChildB()
                                        {
                                            E105_BChildBool = b.Bool,
                                            E105_BChildId = b.id,
                                            E105_BChildLong = b.Long,
                                            E105_BChildString = b.String
                                        }).ToList() ?? new List<E105v1_GotRecentTestData.ChildB>(),
                                        E105_BChild = c.BChild?.Map(b => new E105v1_GotRecentTestData.ChildB()
                                        {
                                            E105_BChildId = b.id,
                                            E105_BChildBool = b.Bool,
                                            E105_BChildLong = b.Long,
                                            E105_BChildString = b.String
                                        }),
                                        E105_CChildId = c.id,
                                        E105_CChildString = c.String
                                    }).ToList() ?? new List<E105v1_GotRecentTestData.ChildC>()
                                }))
                    };

                    await Bus.Publish(
                        response,
                        new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
                }

                async Task<List<TestData>> GetRecentTestData()
                {
                    var recentTestData = await this.Get<TestDataQueries>()
                                                   .Call(x => x.GetRecentTestData(msg.C111_MaxAgeInDays ?? 365, msg.C111_MaxRecords ?? 50))();
                    return recentTestData;
                }
                };
    }
}
