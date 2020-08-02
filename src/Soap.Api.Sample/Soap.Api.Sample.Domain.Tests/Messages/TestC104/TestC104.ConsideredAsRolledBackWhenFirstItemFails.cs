namespace Sample.Tests.Messages
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104CheckConsideredAsRolledBackWhenFirstItemFails : TestC104
    {
        public TestC104CheckConsideredAsRolledBackWhenFirstItemFails(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async void CheckConsideredAsRolledBackWhenFirstItemFails()
        {
            async Task beforeRunHook(DataStore store, int run)
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
                    }

                    void CountDataStoreOperationsSaved(MessageLogEntry log)
                    {
                        log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(2);
                        log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(4); //* includes extra one for this test 
                        log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
                    }
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

            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ConsideredAsRolledBackWhenFirstItemFails);

            try
            {
                await ExecuteWithRetries(
                    c104TestUnitOfWork,
                    Identities.UserOne,
                    1,
                    beforeRunHook); //should succeed on first retry
            }
            catch (PipelineException e)
            {
                e.Message.Should().Contain(GlobalErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack.ToString());
            }
        }
    }
}