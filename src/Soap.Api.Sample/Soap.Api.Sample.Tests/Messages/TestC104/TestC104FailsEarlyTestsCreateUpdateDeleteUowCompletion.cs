//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.TestC104
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
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

            await TestMessage(
                c104TestUnitOfWork,
                Identities.UserOne,
                1,
                (BeforeRunHook, c104TestUnitOfWork.Headers.GetMessageId())
                );

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();
        }

        private async Task BeforeRunHook(DataStore store, int run)
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

        private new void CountDataStoreOperationsSaved(MessageLogEntry log)
        {
            log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(4);
            log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(3);
            log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
        }

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.EventsPublished.Count.Should().Be(1);
        }
    }
}