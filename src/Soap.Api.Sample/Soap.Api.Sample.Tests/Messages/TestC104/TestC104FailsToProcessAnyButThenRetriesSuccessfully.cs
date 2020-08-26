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

    public class TestC104FailsToProcessAnyButThenRetriesSuccessfully : TestC104
    {
        public TestC104FailsToProcessAnyButThenRetriesSuccessfully(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message dies after commiting due to guard failure
           PreRun Assert failure for right reason
           Run 2 retries everything successfully 
        */
        public async void FailsToProcessAnyButThenRetriesSuccessfully()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsToProcessAnyButThenRetriesSuccessfully);

            await TestMessage(c104TestUnitOfWork, Identities.UserOne, 1, beforeRunHook); 

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();
        }

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.EventsPublished.Count.Should().Be(1);
        }

        private async Task beforeRunHook(DataStore store, int run)
        {
            await AssertGuardFail();

            async Task AssertGuardFail()
            {
                if (run == 2)
                {
                    //Assert guard fail
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsToProcessAnyButThenRetriesSuccessfully);
                    var log = await store.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    log.Attempts[0] //* 0 is latest attempt they are inserted
                       .Errors.Errors[0]
                       .message.Should()
                       .Be(SpecialIds.FailsToProcessAnyButThenRetriesSuccessfully.ToString());
                }
            }
        }
    }
}