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

    public class TestC104RollbackSkipsOverItemsUpdatedSinceWeChangedThem : TestC104
    {
        public TestC104RollbackSkipsOverItemsUpdatedSinceWeChangedThem(ITestOutputHelper output)
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
         * Summary: While attempting to rollback we encounter items that have already been updated again since our
         *  update and so we should just skip over those while rolling back.
         *
         * There is an edge case if both updates are in the same UOW but datastore will catch that and collapse
         * the updates before it ever reaches the SOAP code
        */
        public async void RollbackSkipsOverItemsUpdatedSinceWeChangedThem()
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
            await SimulateAnotherUnitOfWorkUpdatingSolosRecord();
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

            async Task SimulateAnotherUnitOfWorkUpdatingSolosRecord()
            {
                if (beforeRunHookArgs.Run == 2)
                {
                    await beforeRunHookArgs.DataStore.UpdateById<UserProfile>(Ids.HanSolo, han => han.FirstName = "Harry");
                    await beforeRunHookArgs.DataStore.CommitChanges();
                }
            }

            async Task AssertRollback()
            {
                if (beforeRunHookArgs.Run == 3)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId(), options => options.ProvidePartitionKeyValues(WeekInterval.FromUtcNow()));
                    var uow = (await beforeRunHookArgs.BlobStorage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();
                    
                    CountDataStoreOperationsSaved(uow);
                    await RecordsShouldBeReturnToOriginalStateExceptSolo(beforeRunHookArgs.DataStore);
                }

                async Task RecordsShouldBeReturnToOriginalStateExceptSolo(DataStore store)
                {
                    //*creations
                    var lando = (await store.Read<UserProfile>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
                    lando.Should().BeNull();

                    var boba = (await store.Read<UserProfile>(x => x.UserName == "boba.fett")).SingleOrDefault();
                    boba.Should().BeNull();

                    //* updates
                    var han = await store.ReadById<UserProfile>(Ids.HanSolo); //* the change to han is not rolled back
                    han.FirstName.Should().Be("Harry");
                    han.LastName.Should().Be("Ford");

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
