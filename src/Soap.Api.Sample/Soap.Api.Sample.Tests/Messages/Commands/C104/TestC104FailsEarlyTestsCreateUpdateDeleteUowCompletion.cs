//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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
                Identities.JohnDoeAllPermissions,
                1,
                (BeforeRunHook, c104TestUnitOfWork.Headers.GetMessageId())
                );

            //assert
            var log = Result.GetMessageLogEntry();
            var uow = Result.GetUnitOfWork();

            
            CountDataStoreOperationsSaved(uow);
            CountMessagesSaved(uow);
            CountMessagesSent();
        }

        private async Task BeforeRunHook(DataStore store, IBlobStorage storage, int run)
        {
            await FixLukesBrokenEtagSoItSucceeds();
            /* this requires you to set the unitofworkid in the executewithretries call above
                 so that this history for lukes record will look like it was saved with the eTag
                 123456 below the first time. */

            async Task FixLukesBrokenEtagSoItSucceeds()
            {
                if (run == 2)
                {
                    await store.UpdateById<UserProfile>(
                        Ids.LukeSkywalker,
                        luke => { luke.Etag = "123456"; },
                        side => side.DisableOptimisticConcurrency());
                    await store.CommitChanges();
                }
            }
        }

        private new void CountDataStoreOperationsSaved(UnitOfWork uow)
        {
            uow.DataStoreCreateOperations.Count.Should().Be(4);
            uow.DataStoreUpdateOperations.Count.Should().Be(3);
            uow.DataStoreDeleteOperations.Count.Should().Be(1);
        }

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.BusEventsPublished.Count.Should().Be(1);
        }
    }
}
