//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.TestC104
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.PfBase.Tests;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104CheckConsideredAsRolledBackWhenFirstItemFails : TestC104
    {
        public TestC104CheckConsideredAsRolledBackWhenFirstItemFails(ITestOutputHelper output)
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

            await TestMessage(c104TestUnitOfWork, Identities.UserOne, 1, (BeforeRunHook,default));

            //assert
            Result.ExceptionContainsErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);
        }

        private async Task BeforeRunHook(DataStore store, int run)
        {
            await Assert();

            async Task Assert()
            {
                if (run == 2)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ConsideredAsRolledBackWhenFirstItemFails);
                    var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    CountDataStoreOperationsSaved(log);
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
                        luke => luke.Auth0Id = Ids.UserOneAuth0Id);
                    await store.CommitChanges();
                }
            }
        }
    }
}
