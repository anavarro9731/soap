namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;

    public class P210ReturnRecentTestData : Process, IBeginProcess<C111v1_GetRecentTestData>
    {
        public Func<C111v1_GetRecentTestData, Task> BeginProcess =>
            async msg =>
                {
                var lastFiveDaysOfEntries = await DataReader.ReadActive<TestData>();
                                                //t => t.CreatedAsMillisecondsEpochTime > DateTime.UtcNow.ConvertToMillisecondsEpochTime() - 86400000 * 5);
                lastFiveDaysOfEntries = lastFiveDaysOfEntries.OrderByDescending(x => x.CreatedAsMillisecondsEpochTime);

                var response = new E105v1_GotRecentTestData
                {
                    E105_TestData = new List<E105v1_GotRecentTestData.TestData>(
                        lastFiveDaysOfEntries.Select(
                            e => new E105v1_GotRecentTestData.TestData
                            {
                                E105_Guid = e.Guid,
                                E105_Id = e.id,
                                E105_Label = e.String,
                                E105_CreatedAt = e.Created
                            }))
                };

                await Publish(response, new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
                };
    }
}
