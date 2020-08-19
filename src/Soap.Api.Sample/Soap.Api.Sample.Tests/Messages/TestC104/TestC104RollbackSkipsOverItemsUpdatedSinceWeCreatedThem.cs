﻿namespace Sample.Tests.Messages
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
            async Task beforeRunHook(DataStore store, int run)
            {
                await SimulateAnotherUnitOfWorkChangingLukesRecord();
                await SimulateAnotherUnitOfWorkUpdatingLandosRecord();
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

                async Task SimulateAnotherUnitOfWorkUpdatingLandosRecord()
                {
                    if (run == 2)
                    {
                        await store.UpdateWhere<User>(
                            u => u.UserName == "lando.calrissian",
                            lando => lando.LastName = "Californian");
                        await store.CommitChanges();
                    }
                }

                async Task AssertRollback()
                {
                    if (run == 3)
                    {
                        //Assert, changes should be rolled back at this point 
                        var c104TestUnitOfWork =
                            Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem);
                        var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                        CountDataStoreOperationsSaved(log);
                        await RecordsShouldBeReturnToOriginalStateExceptLando(store);
                    }

                    async Task RecordsShouldBeReturnToOriginalStateExceptLando(DataStore store)
                    {
                        //*creations
                        var lando = (await store.Read<User>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
                        lando.Should().NotBeNull();
                        lando.LastName.Should().Be("Californian");

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
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem);

            try
            {
                await ExecuteWithRetries(
                    c104TestUnitOfWork,
                    Identities.UserOne,
                    2,
                    beforeRunHook); 
            }
            catch (PipelineException e)
            {
                e.Message.Should().Contain(GlobalErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack.ToString());
            }
        }
    }
}