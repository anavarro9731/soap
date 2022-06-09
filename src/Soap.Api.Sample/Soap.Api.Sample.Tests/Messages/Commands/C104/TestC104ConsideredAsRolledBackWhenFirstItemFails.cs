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

    public class TestC104ConsideredAsRolledBackWhenFirstItemFails : TestC104
    {
        public TestC104ConsideredAsRolledBackWhenFirstItemFails(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /*  Run 1 etag failure on the first item in the uow
            PreRun Check no records were changed
            Run 2 won't process because it see's first record as uncommittable an considers everything as rolled back
         */
        public async void CheckConsideredAsRolledBackWhenFirstItemFails()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ConsideredAsRolledBackWhenFirstItemFails);

            await TestMessage(c104TestUnitOfWork, Identities.JohnDoeAllPermissions, 1, (BeforeRunHook,default));

            //assert
            Result.Success.Should().BeFalse();
            Result.MessageBus.CommandsSent.Single().Should().BeOfType<MessageFailedAllRetries>();
            Result.ExceptionContainsErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);
        }

        private async Task BeforeRunHook(DataStore store, IBlobStorage storage, int run)
        {
            await Assert();

            async Task Assert()
            {
                if (run == 2)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ConsideredAsRolledBackWhenFirstItemFails);
                    
                    var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    var uow = (await storage.GetBlobOrError(c104TestUnitOfWork.Headers.GetMessageId(), "units-of-work")).ToUnitOfWork();
                    
                    CountDataStoreOperationsSaved(uow);
                    await RecordsShouldBeReturnToOriginalState(store);
                    await SimulateAnotherUnitOfWorkChangingLukesRecord();
                }
            }

            async Task SimulateAnotherUnitOfWorkChangingLukesRecord()
            {
                //* luke's record is the only one not yet committed but since we faked a concurrency error in
                //the change we now need to reflect that in the underlying data for the next run to calculate correctly
                if (run == 2)
                {
                    await store.UpdateById<UserProfile>(
                        Ids.LukeSkywalker,
                        luke => luke.Auth0Id = Ids.JohnDoeWithAllPermissionsAuth0Id);
                    await store.CommitChanges();
                }
            }
        }
    }
}
