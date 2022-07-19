//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
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

    public class TestC104RollbackSkipsOverItemsDeletedSinceWeCreatedThem : TestC104
    {
        public TestC104RollbackSkipsOverItemsDeletedSinceWeCreatedThem(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message dies after commiting uow due to eTag violation
         * Prerun fix underlying data to match with etag violation in run 1 and delete solos record 
         * Run 2 sees it will be unable to retry starts rolling everything back but skipping over landos record leaving the delete in place
         * Prerun Assert Rollback
         * Run 3 message fails all items rolled back except landos
         * 
         * Summary: While attempting to rollback we encounter items that have been hard deleted since we
         * created them so we should just skip over those while rolling back, what actually happens is that
         * the system thinks they haven't been created yet and so ignore them entirely during the rollback
         *
         * There is an edge case if the delete and create are in the same UOW but Datastore will pick that up
         * and remove the item from its uow when the delete is requested so that wont happen */
        public async void RollbackSkipsOverItemsDeletedSinceWeCreatedThem()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsDeletedSinceWeCreatedThem);

            await TestMessage(c104TestUnitOfWork, Identities.JohnDoeAllPermissions, 2, (BeforeRunHook,default));
            
            //assert
            Result.ExceptionContainsErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);
        }

        private async Task BeforeRunHook(SoapMessageTestContext.BeforeRunHookArgs beforeRunHookArgs)
        {
            await SimulateAnotherUnitOfWorkChangingLukesRecord();
            await SimulateAnotherUnitOfWorkDeletingLandosRecord();
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

            async Task SimulateAnotherUnitOfWorkDeletingLandosRecord()
            {
                if (beforeRunHookArgs.Run == 2)
                {
                    await beforeRunHookArgs.DataStore.DeleteWhere<UserProfile>(u => u.UserName == "lando.calrissian", options => options.Permanently());
                    await beforeRunHookArgs.DataStore.CommitChanges();
                }
            }

            async Task AssertRollback()
            {
                if (beforeRunHookArgs.Run == 3)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsDeletedSinceWeCreatedThem);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId(), options => options.ProvidePartitionKeyValues(WeekInterval.FromUtcNow()));
                    var uow = (await beforeRunHookArgs.BlobStorage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();
                    CountDataStoreOperationsSaved(uow);
                    await RecordsShouldBeReturnToOriginalStateExceptLando(beforeRunHookArgs.DataStore);
                }

                async Task RecordsShouldBeReturnToOriginalStateExceptLando(DataStore store)
                {
                    //*creations
                    var lando = (await store.Read<UserProfile>(x => x.UserName == "lando.calrissian"))
                        .SingleOrDefault(); //* should ignore original state
                    lando.Should().BeNull();

                    var boba = (await store.Read<UserProfile>(x => x.UserName == "boba.fett")).SingleOrDefault();
                    boba.Should().BeNull();

                    //* updates
                    var han = await store.ReadById<UserProfile>(Ids.HanSolo);
                    han.FirstName.Should().Be(Aggregates.HanSolo.FirstName);
                    han.LastName.Should().Be(Aggregates.HanSolo.LastName);

                    var leia = await store.ReadById<UserProfile>(Ids.PrincessLeia);
                    leia.Active.Should().BeTrue();

                    //* deletes
                    var darth = await store.ReadById<UserProfile>(Ids.DarthVader);
                    darth.Should().NotBeNull();

                    var luke = await store.ReadById<UserProfile>(Ids.LukeSkywalker);
                    luke.Should().NotBeNull();
                }
            }
        }
    }
}
