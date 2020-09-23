//*     $$REMOVE-IN-COPY$$
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
    using Xunit;
    using Xunit.Abstractions;

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

            await TestMessage(c104TestUnitOfWork, Identities.UserOne, 2, (BeforeRunHook,default));

            //assert
            Result.UnhandledError.Message.Should().Contain(GlobalErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack.ToString());
        }

        private async Task BeforeRunHook(DataStore store, int run)
        {
            await SimulateAnotherUnitOfWorkChangingLukesRecord();
            await AssertRollback();

            async Task SimulateAnotherUnitOfWorkChangingLukesRecord()
            {
                //* luke's record is the only one not yet committed but since we faked a concurrency error in
                //the change we now need to reflect that in the underlying data for the next run to calculate correctly
                if (run == 2)
                {
                    await store.UpdateById<User>(
                        Ids.LukeSkywalker,
                        luke => luke.Roles.Add(
                            new Role
                            {
                                Name = "doesnt matter just make a change to add a history item"
                            }));
                    await store.CommitChanges();
                }
            }

            async Task AssertRollback()
            {
                if (run == 3)
                {
                    //Assert, changes should be rolled back at this point 
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ProcessesSomeThenRollsBackSuccessfully);
                    var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    CountDataStoreOperationsSaved(log);
                    await RecordsShouldBeReturnToOriginalState(store);
                }
            }
        }
    }
}