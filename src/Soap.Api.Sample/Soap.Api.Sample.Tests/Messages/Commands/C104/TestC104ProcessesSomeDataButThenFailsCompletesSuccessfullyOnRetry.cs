//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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
            var uow = Result.GetUnitOfWork();
            CountDataStoreOperationsSaved(uow);
            CountMessagesSaved(uow);
            CountMessagesSent();
        }

        private async Task BeforeRunHook(SoapMessageTestContext.BeforeRunHookArgs beforeRunHookArgs)
        {
            await FixSolosBrokenEtagSoItSucceeds();

            async Task FixSolosBrokenEtagSoItSucceeds()
            {
                if (beforeRunHookArgs.Run == 2)
                {
                    await beforeRunHookArgs.DataStore.UpdateById<UserProfile>(
                        Ids.HanSolo,
                        luke => { luke.Etag = "123456"; },
                        side => side.DisableOptimisticConcurrency());
                    await beforeRunHookArgs.DataStore.CommitChanges();
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
