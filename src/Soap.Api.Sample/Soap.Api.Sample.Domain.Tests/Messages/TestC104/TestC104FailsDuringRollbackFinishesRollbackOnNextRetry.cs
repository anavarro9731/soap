namespace Sample.Tests.Messages
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Xunit;
    using Xunit.Abstractions;

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
            async Task beforeRunHook(DataStore store, int run)
            {
                await SimulateAnotherUnitOfWorkChangingLukesRecord();
                await AssertGuardFail();
                await AssertRollback();

                async Task SimulateAnotherUnitOfWorkChangingLukesRecord()
                {
                    //* luke's record is the only one not yet committed but since we faked a concurrency error in
                    //the change we now need to reflect that in the underlying data for the next run to calculate correctly
                    if (run == 2)
                    {
                        await store.UpdateById<User>(
                            Ids.LukeSkywalker,
                            luke => luke.Roles.Add(new Role { Name = "doesnt matter just make a change to add a history item" }));
                        await store.CommitChanges();
                    }
                }

                async Task AssertGuardFail()
                {
                    if (run == 3)
                    {
                        //Assert guard fail
                        var c104TestUnitOfWork =
                            Commands.TestUnitOfWork(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry);
                        var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                        log.Attempts[0] //* 0 is latest attempt they are inserted
                           .Errors.Errors[0]
                           .message.Should()
                           .Be(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry.ToString());
                    }
                }

                async Task AssertRollback()
                {
                    if (run == 4)
                    {
                        //Assert, changes should be rolled back at this point 
                        var c104TestUnitOfWork =
                            Commands.TestUnitOfWork(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry);
                        var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                        CountDataStoreOperationsSaved(log);
                        await RecordsShouldBeReturnToOriginalState(store);
                    }

                    async Task RecordsShouldBeReturnToOriginalState(DataStore store)
                    {
                        //*creations
                        var lando = (await store.Read<User>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
                        lando.Should().BeNull();

                        var boba = (await store.Read<User>(x => x.UserName == "boba.fett")).SingleOrDefault();
                        boba.Should().BeNull();

                        //* updates
                        var han = await store.ReadById<User>(Ids.HanSolo);
                        han.FirstName.Should().Be(Aggregates.HanSolo.FirstName);
                        han.LastName.Should().Be(Aggregates.HanSolo.LastName);

                        var leia = await store.ReadById<User>(Ids.PrincessLeia);
                        leia.Active.Should().BeTrue();

                        //* deletes
                        var darth = await store.ReadById<User>(Ids.DarthVader);
                        darth.Should().NotBeNull();

                        var luke = await store.ReadById<User>(Ids.LukeSkywalker);
                        luke.Should().NotBeNull();
                    }
                }
            }

            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry);

            try
            {
                await ExecuteWithRetries(
                    c104TestUnitOfWork,
                    Identities.UserOne,
                    2,
                    beforeRunHook); //should succeed on first retry
            }
            catch (PipelineException e)
            {
                e.Message.Should().Contain(GlobalErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack.ToString());
            }
        }
    }
}