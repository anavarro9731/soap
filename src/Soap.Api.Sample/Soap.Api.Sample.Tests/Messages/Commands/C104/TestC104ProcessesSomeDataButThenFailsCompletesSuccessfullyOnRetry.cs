//* ##REMOVE-IN-COPY##
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

            await TestMessage(
                c104TestUnitOfWork,
                Identities.JohnDoeAllPermissions,
                1,
                (BeforeRunHook,
                c104TestUnitOfWork.Headers.GetMessageId()));

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();
        }

        private async Task BeforeRunHook(DataStore store, int run)
        {
            await FixSolosBrokenEtagSoItSucceeds();

            async Task FixSolosBrokenEtagSoItSucceeds()
            {
                if (run == 2)
                {
                    await store.UpdateById<UserProfile>(
                        Ids.HanSolo,
                        luke => { luke.Etag = "123456"; },
                        side => side.DisableOptimisticConcurrency());
                    await store.CommitChanges();
                }
            }
        }

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.BusEventsPublished.Count.Should().Be(1);
        }
    }
}
