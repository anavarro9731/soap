namespace Sample.Tests.Messages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
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

            ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

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
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.RetryHappyPath);

            ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            RecordsShouldBeReturnToOriginalState();
            NoMessagesShouldBeSent();

            async Task RecordsShouldBeReturnToOriginalState()
            {
                //*creations
                var lando = (await Result.DataStore.Read<User>(x => x.UserName == "lando.calrissian")).SingleOrDefault();
                lando.Should().BeNull();
                
                var boba = (await Result.DataStore.Read<User>(x => x.UserName == "boba.fett")).SingleOrDefault();
                boba.Should().BeNull();
                
                //* updates
                var han = await Result.DataStore.ReadById<User>(Ids.HanSolo);
                han.FirstName.Should().Be(Aggregates.HanSolo.FirstName);
                han.LastName.Should().Be(Aggregates.HanSolo.LastName);
                
                var leia = await Result.DataStore.ReadById<User>(Ids.PrincessLeia);
                leia.Active.Should().BeTrue();
                
                //* deletes
                var darth = await Result.DataStore.ReadById<User>(Ids.DarthVader);
                darth.Should().NotBeNull();
                
                var luke = await Result.DataStore.ReadById<User>(Ids.LukeSkywalker);
                luke.Should().NotBeNull();

            }

            void NoMessagesShouldBeSent()
            {
                Result.MessageBus.CommandsSent.Should().BeEmpty();
                Result.MessageBus.EventsPublished.Should().BeEmpty();
            }
        }

        [Fact]
        /* message is retried max times and thrown out as poison when there is an error
        saving the unit of work */
        public void CheckThatMessageDiesNormallyWhenUnitOfWorkIsNotSaved()
        {
            //act
            try
            {
                ExecuteWithRetries(
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

            ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
        }
        
        private void CountDataStoreOperationsSaved(MessageLogEntry log)
        {
            log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(2);
            log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(2); //* includes soft delete
            log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(2);
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