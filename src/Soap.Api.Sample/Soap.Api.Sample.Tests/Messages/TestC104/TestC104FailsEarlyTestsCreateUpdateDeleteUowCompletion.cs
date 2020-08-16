namespace Sample.Tests.Messages
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

    public class TestC104FailsEarlyInReplayThenCompletesRemainderOfUow : TestC104
    {
        public TestC104FailsEarlyInReplayThenCompletesRemainderOfUow(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /*
         * Run 1 message fails on eTag violation
         * PreRun change underlying record so it reflects the new eTag removing the violation
         * Run 2 message succeeds eTag violation removed
         */
        public async void FailsEarlyInReplayThenCompletesRemainderOfUow()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsEarlyInReplayThenCompletesRemainderOfUow);

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

            void CountDataStoreOperationsSaved(MessageLogEntry log)
            {
                log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(4);
                log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(3);
                log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
            }

            async Task beforeRunHook(DataStore store, int run)
            {
                await FixLukesBrokenEtagSoItSucceeds();
                /* this requires you to set the unitofworkid in the executewithretries call above
                 so that this history for lukes record will look like it was saved with the eTag
                 123456 below the first time. */

                async Task FixLukesBrokenEtagSoItSucceeds()
                {
                    if (run == 2)
                    {
                        await store.UpdateById<User>(
                            Ids.LukeSkywalker,
                            luke => { luke.Etag = "123456"; },
                            side => side.DisableOptimisticConcurrency());
                        await store.CommitChanges();
                    }
                }
            }
        }
    }
}