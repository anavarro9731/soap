//*     $$REMOVE-IN-COPY$$
namespace Soap.Api.Sample.Tests.Messages.TestC104
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
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

            await TestMessage(c104TestUnitOfWork, Identities.UserOne, 1, (BeforeRunHook,default));

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
            CountMessagesSent();
        }

        private async Task BeforeRunHook(DataStore store, int run)
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
                       .Be(SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully.ToString());

                    log.ProcessingComplete.Should().BeFalse();
                }
            }
        }

        private void CountMessagesSent()
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
    }
}