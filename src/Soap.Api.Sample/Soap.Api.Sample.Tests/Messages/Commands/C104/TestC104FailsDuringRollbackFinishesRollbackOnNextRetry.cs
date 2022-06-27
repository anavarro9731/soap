//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
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

    public class TestC104FailsDuringRollbackFinishesRollbackOnNextRetry : TestC104
    {
        public TestC104FailsDuringRollbackFinishesRollbackOnNextRetry(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /*
         * Run 1 message dies after commiting uow due to eTag violation
         * Prerun fix underlying data to match with etag violation in run 1
         * Run 2 sees it will be unable to retry starts rolling everything back fails rolling back due to test guard
         * Prerun check run failed for the right reason
         * Run 3 finally completes rollback
         * PreRun assert rollback in Run 3
         * Run 4 message fails all items rolled back*/
        public async void FailsDuringRollbackFinishesRollbackOnNextRetry()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry);

            await TestMessage(c104TestUnitOfWork, Identities.JohnDoeAllPermissions, 3, (BeforeRunHook,default));

            //assert
            Result.ExceptionContainsErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);
        }

        private async Task BeforeRunHook(SoapMessageTestContext.BeforeRunHookArgs beforeRunHookArgs)
        {
            await SimulateAnotherUnitOfWorkChangingLukesRecord();
            await AssertGuardFail();
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

            async Task AssertGuardFail()
            {
                if (beforeRunHookArgs.Run == 3)
                {
                    //Assert guard fail
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    var uow = (await beforeRunHookArgs.BlobStorage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();


                               
                    log.Attempts[0] //* 0 is latest attempt they are inserted
                       .Errors.AllErrors[0]
                       .ExternalMessage.Should()
                       .Be(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry.ToString());
                }
            }

            async Task AssertRollback()
            {
                if (beforeRunHookArgs.Run == 4)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    var uow = (await beforeRunHookArgs.BlobStorage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();


                    CountDataStoreOperationsSaved(uow);
                    await RecordsShouldBeReturnToOriginalState(beforeRunHookArgs.DataStore);
                }
            }
        }
    }
}
