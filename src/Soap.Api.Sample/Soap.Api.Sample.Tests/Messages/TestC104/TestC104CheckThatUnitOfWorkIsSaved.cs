//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.TestC104
{
    using Soap.Context.Logging;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104CheckTheUnitOfWorkIsSavedCorrectly : TestC104
    {
        public TestC104CheckTheUnitOfWorkIsSavedCorrectly(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* all items queued during processing appear successfully in the unit of work
         even though this test doesn't cause the message to fail or retry the uow is still
         saved in case it had. the uow has no state property because keeping that consistent
         would add unimaginable complexity (it is always calculated at runtime) */
        public async void CheckTheUnitOfWorkIsSavedCorrectly()
        {
            //act
            SetupTestByProcessingAMessage(Commands.TestUnitOfWork(), Identities.UserOne);

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(
                          Commands.TestUnitOfWork().Headers.GetMessageId());
            CountMessagesSaved(log);
            CountDataStoreOperationsSaved(log);
        }
    }
}