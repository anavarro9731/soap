namespace Sample.Tests.Messages
{
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
        //* data complete bus messages not complete, so send the incomplete ones
        public async void ProcessesDataButFailsBeforeMessagesRetriesSuccessfully()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
        }
    }
}