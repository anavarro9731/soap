//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC104ProcessesSomeThenRollsBackSuccessfully : TestC104
    {
        public TestC104ProcessesSomeThenRollsBackSuccessfully(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message dies after commiting uow due to eTag violation
        * Prerun fix underlying data to match with etag violation in run 1
        * Run 2 sees it will be unable to retry starts rolling everything 
        * Prerun Assert Rollback
        * Run 3 message fails all items rolled back
        */
        public async void ProcessesSomeThenRollsBackSuccessfully()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ProcessesSomeThenRollsBackSuccessfully);

            await TestMessage(c104TestUnitOfWork, Identities.JohnDoeAllPermissions, 2, (BeforeRunHook,default));

            //assert
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E001v1_MessageFailed>();
            Result.ExceptionContainsErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);
        }

        private async Task BeforeRunHook(SoapMessageTestContext.BeforeRunHookArgs beforeRunHookArgs)
        {
            await SimulateAnotherUnitOfWorkChangingLukesRecord();
            await AssertRollback();

            async Task SimulateAnotherUnitOfWorkChangingLukesRecord()
            {
                //* luke's record is the only one not yet committed but since we faked a concurrency error in
                //the change we now need to reflect that in the underlying data for the next run to calculate correctly
                if (beforeRunHookArgs.Run == 2)
                {
                    await beforeRunHookArgs.DataStore.UpdateById<UserProfile>(
                        Ids.LukeSkywalker,
                        luke => luke.AnyString = "changed"); //doesn't matter just make any change to create a history item, but avoid ID fields or sensitive fields used elsewhere in framework
                    await beforeRunHookArgs.DataStore.CommitChanges();
                }
            }

            async Task AssertRollback()
            {
                if (beforeRunHookArgs.Run == 3)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ProcessesSomeThenRollsBackSuccessfully);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    var uow = (await beforeRunHookArgs.BlobStorage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();
                    CountDataStoreOperationsSaved(uow);
                    await RecordsShouldBeReturnToOriginalState(beforeRunHookArgs.DataStore);
                }
            }
        }
    }
}
