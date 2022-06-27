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

    public class TestC104RollbackSkipsOverItemsUpdatedSinceWeCreatedThem : TestC104
    {
        public TestC104RollbackSkipsOverItemsUpdatedSinceWeCreatedThem(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message dies after commiting uow due to eTag violation
         * Prerun fix underlying data to match with etag violation in run 1 and update solos record 
         * Run 2 sees it will be unable to retry starts rolling everything back but skipping over solos record leaving the last update in place
         * Prerun Assert Rollback
         * Run 3 message fails all items rolled back except solos
         * 
         * Summary: While attempting to rollback on the second retry we encounter items that have already been updated again since we
        created them and so we should just skip over those while rolling back. 
        
        There is an edge case where the update and create for an item are in the same uow when there is a rollback. 
        This could cause a lot of problems but Datastore collapses theses types of changes internally so need to worry about that */
        public async void RollbackSkipsOverItemsUpdatedSinceWeCreatedThem()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem);

            await TestMessage(c104TestUnitOfWork, Identities.JohnDoeAllPermissions, 2, (BeforeRunHook,default));

            //assert
            Result.ExceptionContainsErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);
        }

        private async Task BeforeRunHook(SoapMessageTestContext.BeforeRunHookArgs beforeRunHookArgs)
        {
            await SimulateAnotherUnitOfWorkChangingLukesRecord();
            await SimulateAnotherUnitOfWorkUpdatingLandosRecord();
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

            async Task SimulateAnotherUnitOfWorkUpdatingLandosRecord()
            {
                if (beforeRunHookArgs.Run == 2)
                {
                    await beforeRunHookArgs.DataStore.UpdateWhere<UserProfile>(u => u.UserName == "lando.calrissian", lando => lando.LastName = "Californian");
                    await beforeRunHookArgs.DataStore.CommitChanges();
                }
            }

            async Task AssertRollback()
            {
                if (beforeRunHookArgs.Run == 3)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem);
                    
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    var uow = (await beforeRunHookArgs.BlobStorage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();

                    CountDataStoreOperationsSaved(uow);
                    await RecordsShouldBeReturnToOriginalStateExceptLando(beforeRunHookArgs.DataStore);
                }

                async Task RecordsShouldBeReturnToOriginalStateExceptLando(DataStore store)
                {
                    //*creations
                    var lando = (await store.Read<UserProfile>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
                    lando.Should().NotBeNull();
                    lando.LastName.Should().Be("Californian");

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
