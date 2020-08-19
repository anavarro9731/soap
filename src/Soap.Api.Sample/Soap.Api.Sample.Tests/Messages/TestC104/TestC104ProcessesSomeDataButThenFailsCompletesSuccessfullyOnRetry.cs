﻿namespace Sample.Tests.Messages
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry : TestC104
    {
        public TestC104ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message dies with eTag failure
           PreRun change underlying record so it reflects the new eTag removing the violation
           Run 2 Retry completes successfully 
        */
        public async void ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry()
        {
            //act
            var c104TestUnitOfWork =
                Commands.TestUnitOfWork(SpecialIds.ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry);

            await ExecuteWithRetries(
                c104TestUnitOfWork,
                Identities.UserOne,
                1,
                beforeRunHook,
                c104TestUnitOfWork.Headers.GetMessageId()); 

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();

            void CountMessagesSent()
            {
                Result.MessageBus.CommandsSent.Count.Should().Be(1);
                Result.MessageBus.EventsPublished.Count.Should().Be(1);
            }

            async Task beforeRunHook(DataStore store, int run)
            {
                await FixSolosBrokenEtagSoItSucceeds();

                async Task FixSolosBrokenEtagSoItSucceeds()
                {
                    if (run == 2)
                    {
                        await store.UpdateById<User>(
                            Ids.HanSolo,
                            luke => { luke.Etag = "123456"; },
                            side => side.DisableOptimisticConcurrency());
                        await store.CommitChanges();
                    }
                }
            }
        }
    }
}