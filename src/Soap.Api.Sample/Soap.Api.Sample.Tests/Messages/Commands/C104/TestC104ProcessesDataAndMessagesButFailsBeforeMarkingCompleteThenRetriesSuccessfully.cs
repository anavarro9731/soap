//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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
                    var c104TestUnitOfWork = Commands.TestUnitOfWork(
                        SpecialIds.ProcessesDataAndMessagesButFailsBeforeMarkingCompleteThenRetriesSuccessfully);
                    var log = await beforeRunHookArgs.DataStore.ReadById<MessageLogEntry>(c104TestUnitOfWork.Headers.GetMessageId(), options => options.ProvidePartitionKeyValues(WeekInterval.FromUtcNow()));
                    log.Attempts[0] //* 0 is latest attempt they are inserted
                       .Errors.AllErrors[0]
                       .ExternalMessage.Should()
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
            Result.MessageBus.BusEventsPublished.Count.Should().Be(2); //* duplicates sent
            Result.MessageBus.BusEventsPublished[0]
                  .Headers.GetMessageId()
                  .Should()
                  .Be(Result.MessageBus.BusEventsPublished[1].Headers.GetMessageId()); //* duplicates sent
        }
    }
}
