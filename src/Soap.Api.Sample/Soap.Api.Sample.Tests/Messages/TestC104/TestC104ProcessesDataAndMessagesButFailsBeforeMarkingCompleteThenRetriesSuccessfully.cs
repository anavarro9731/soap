namespace Sample.Tests.Messages
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully : TestC104
    {
        public TestC104ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 data complete bus messages not started due to guard failure
           RUn 2 complete messages
           */
        public async void ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(
                SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1, beforeRunHook); 

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();

            void CountMessagesSent()
            {
                Result.MessageBus.CommandsSent.Count.Should().Be(2);
                Result.MessageBus.CommandsSent[0]
                      .Headers.GetMessageId()
                      .Should()
                      .Be(Result.MessageBus.CommandsSent[1].Headers.GetMessageId());
                Result.MessageBus.EventsPublished.Count.Should().Be(2); //* duplicates sent
                Result.MessageBus.EventsPublished[0]
                      .Headers.GetMessageId()
                      .Should()
                      .Be(Result.MessageBus.EventsPublished[1].Headers.GetMessageId()); //* duplicates sent
            }

            async Task beforeRunHook(DataStore store, int run)
            {
                await AssertGuardFail();

                async Task AssertGuardFail()
                {
                    if (run == 2)
                    {
                        //Assert guard fail
                        var c104TestUnitOfWork = Commands.TestUnitOfWork(
                            SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully);
                        var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                        log.Attempts[0] //* 0 is latest attempt they are inserted
                           .Errors.Errors[0]
                           .message.Should()
                           .Be(
                               SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully
                                         .ToString());

                        log.ProcessingComplete.Should().BeFalse();
                    }
                }
            }
        }
    }
}