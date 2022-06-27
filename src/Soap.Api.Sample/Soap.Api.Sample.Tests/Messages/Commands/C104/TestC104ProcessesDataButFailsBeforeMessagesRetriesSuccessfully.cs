//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Threading.Tasks;
    using DataStore;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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

            await TestMessage(c104TestUnitOfWork, Identities.JohnDoeAllPermissions, 1, (BeforeRunHook,default));

            //assert
            var uow = Result.GetUnitOfWork();
            CountDataStoreOperationsSaved(uow);
            CountMessagesSaved(uow);
            CountMessagesSent();
        }

        private async Task BeforeRunHook(SoapMessageTestContext.BeforeRunHookArgs beforeRunHookArgs)
        {
            await AssertGuardFail();

            async Task AssertGuardFail()
            {
                if (beforeRunHookArgs.Run == 2)
                {
                    //Assert guard fail
                    var c104TestUnitOfWork =
                        Commands.TestUnitOfWork(SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId());
                    log.Attempts[0] //* 0 is latest attempt they are inserted
                       .Errors.AllErrors[0]
                       .ExternalMessage.Should()
                       .Be(SpecialIds.ProcessesDataButFailsBeforeMessagesRetriesSuccessfully.ToString());

                    log.ProcessingComplete.Should().BeFalse();
                }
            }
        }

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.BusEventsPublished.Count.Should().Be(1);
        }
    }
}
