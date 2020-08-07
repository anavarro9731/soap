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

    public class TestC104ProcessesDataButFailsBeforeMessagesRetriesSuccessfully : TestC104
    {
        public TestC104ProcessesDataButFailsBeforeMessagesRetriesSuccessfully(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 data complete bus messages not started due to guard failure
           RUn 2 complete messages
           */
        public async void ProcessesDataButFailsBeforeMessagesRetriesSuccessfully()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();

            void CountMessagesSent()
            {
                Result.MessageBus.CommandsSent.Count.Should().Be(1);
                Result.MessageBus.EventsPublished.Count.Should().Be(1);
            }

            async Task beforeRunHook(DataStore store, int run)
            {
                await AssertGuardFail();

                async Task AssertGuardFail()
                {
                    if (run == 2)
                    {
                        //Assert guard fail
                        var c104TestUnitOfWork =
                            Commands.TestUnitOfWork(SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully);
                        var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                        log.Attempts[0] //* 0 is latest attempt they are inserted
                           .Errors.Errors[0]
                           .message.Should()
                           .Be(SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully.ToString());

                        log.ProcessingComplete.Should().BeFalse();
                    }
                }
            }
        }
    }
}