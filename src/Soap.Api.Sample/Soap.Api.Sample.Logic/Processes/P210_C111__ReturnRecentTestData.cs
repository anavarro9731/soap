//* ##REMOVE-IN-COPY##

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
                                    E105_Html = "TESTHTML",
                                    E105_Label = "TESTLABEL",
                                    E105_CreatedAt = e.Created,
                                    E105_Countries = e.Countries?.Select(c => new E105v1_GotRecentTestData.Country()
                                    {
                                        E105_Cities = c.Cities?.Select(b => new E105v1_GotRecentTestData.City()
                                        {
                                            E105_HasCathedral = b.Bool,
                                            E105_CityId = b.id,
                                            E105_Population = b.Long,
                                            E105_Name = b.String
                                        }).ToList() ?? new List<E105v1_GotRecentTestData.City>(),
                                        E105_CapitalCity = c.CapitalCity?.Map(b => new E105v1_GotRecentTestData.City()
                                        {
                                            E105_CityId = b.id,
                                            E105_HasCathedral = b.Bool,
                                            E105_Population = b.Long,
                                            E105_Name = b.String
                                        }),
                                        E105_CountryId = c.id,
                                        E105_Name2 = c.String
                                    }).ToList() ?? new List<E105v1_GotRecentTestData.Country>()
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
