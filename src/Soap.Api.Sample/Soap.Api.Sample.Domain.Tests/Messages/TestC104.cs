namespace Sample.Tests.Messages
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Microsoft.Azure.Amqp.Framing;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Operations;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104 : Test
    {
        public TestC104(ITestOutputHelper output)
            : base(output)
        {
            //arrange
            Add(Aggregates.HanSolo);
            Add(Aggregates.DarthVader);
            Add(Aggregates.PrincessLeia);
            Add(Aggregates.LukeSkywalker);
        }

        [Fact]
        //* message dies after commiting uow and then retries everything successfully on the first retry attempt
        public async void CheckRetryWorks()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RetryHappyPath);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
        }

        [Fact]
        //* message dies after commiting uow and then retries everything but fails on the last item so it rolls everything
        //back successfully
        public async void CheckRollbackWorks()
        {
            async Task beforeRunHook(DataStore store, int run)
            {
                await SimulateAnotherUnitOfWorkChangingLukesRecord();
                await Assert();
                
                async Task SimulateAnotherUnitOfWorkChangingLukesRecord()
                {
                    //* lukes record is the only one not yet committed but since we faked a concurrency error in
                    //the change we now need to reflect that in the underlying data for the next run
                    if (run == 2 /* second run = first retry */)
                    {
                        //SimulateAnotherUnitOfWorkChangingLukesRecord
                        await store.UpdateById<User>(
                                Ids.LukeSkywalker,
                                luke => luke.Roles.Add(
                                    new Role { Name = "doesnt matter just make a change to add a history item" }))
                            ;
                        await store.CommitChanges();
                    } 
                }

                async Task Assert()
                {
                    if (run == 3 /* second run = first retry */)
                    {
                        //Assert, changes should be rolled back at this point 
                        var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackHappyPath);
                        var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                        CountDataStoreOperationsSaved(log);
                        await RecordsShouldBeReturnToOriginalState(store);
                    } 
                }
            } 
            
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RollbackHappyPath);

            try
            {
                await ExecuteWithRetries(
                    c104TestUnitOfWork,
                    Identities.UserOne,
                    3,
                    beforeRunHook); //should succeed on first retry
            }
            catch (PipelineException e)
            {
                e.Message.Should().Contain(GlobalErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack.ToString());
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

        [Fact]
        /* message is retried max times and thrown out as poison when there is an error
        saving the unit of work */
        public async void CheckThatMessageDiesNormallyWhenUnitOfWorkIsNotSaved()
        {
            //act
            try
            {
                await ExecuteWithRetries(
                    Commands.TestUnitOfWork(SpecialIds.MessageThatDiesWhileSavingUnitOfWork),
                    Identities.UserOne,
                    3);
                
                throw new Exception("Should not reach this");
            }
            catch (Exception e)
            {
                e.Message.Should().Contain(SpecialIds.MessageThatDiesWhileSavingUnitOfWork.ToString());
            }

            ;
        }

        [Fact]
        /* all items queued during processing appear successfully in the unit of work
         even though this test doesn't cause the message to fail or retry the uow is still
         saved in case it had. the uow has no state property because keeping that consistent
         would add unimaginable complexity (it is always calculated at runtime) */
        public async void CheckTheUnitOfWorkIsSavedCorrectly()
        {
            //act
            Execute(Commands.TestUnitOfWork(), Identities.UserOne);

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(Commands.TestUnitOfWork().Headers.GetMessageId());
            CountMessagesSaved(log);
            CountDataStoreOperationsSaved(log);
        }

        [Fact]
        //* data complete bus messages not complete, so send the incomplete ones
        public async void CheckIncompleteMessagesAreFinished()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.SendUnsentMessages);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
        }
        
        private void CountDataStoreOperationsSaved(MessageLogEntry log)
        {
            log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(2);
            log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(3); //* includes soft delete
            log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
        }

        private void CountMessagesSaved(MessageLogEntry log)
        {
            var c100wrapped = log.UnitOfWork.BusCommandMessages.Single();
            var c100 = c100wrapped.Deserialise<C100Ping>();
            c100.PingedBy.Should().Be(nameof(P555TestUnitOfWork));

            var e150wrapped = log.UnitOfWork.BusEventMessages.Single();
            var e150 = e150wrapped.Deserialise<E150Pong>();
            e150.PongedBy.Should().Be(nameof(P555TestUnitOfWork));
        }
    }
}