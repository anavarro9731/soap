namespace Sample.Tests.Messages
{
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
        //* message dies after commiting uow and then retries everything successfully on the first retry attempt
        public async void FailsToProcessAnyButThenRetriesSuccessfully()
        {
            //act
            var c104TestUnitOfWork = Commands.TestUnitOfWork(SpecialIds.FailsToProcessAnyButThenRetriesSuccessfully);

            await ExecuteWithRetries(c104TestUnitOfWork, Identities.UserOne, 1); //should succeed on first retry

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
            CountDataStoreOperationsSaved(log);
            CountMessagesSaved(log);
        }
    }
}