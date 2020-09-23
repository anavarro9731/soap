//*     ##REMOVE-IN-COPY##
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

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.EventsPublished.Count.Should().Be(1);
        }
    }
}