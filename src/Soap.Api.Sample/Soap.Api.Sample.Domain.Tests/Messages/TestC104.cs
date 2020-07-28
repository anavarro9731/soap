namespace Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104 : BaseTest
    {
        public TestC104(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async void ItShouldPersistTheItemsAddedInSession()
        {
            //arrange
            Add(Aggregates.HansSolo);
            Add(Aggregates.DarthVader);

            //act
            Execute(Commands.TestUnitOfWork, Identities.UserOne);

            //assert
            var log = await Result.DataStore.ReadById<MessageLogEntry>(Commands.TestUnitOfWork.Headers.GetMessageId());
            DataStoreOperationsSaved();
            MessagesSaved();

            void DataStoreOperationsSaved()
            {
                log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(2);
                log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(1);
                log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
            }

            void MessagesSaved()
            {
                var c100wrapped = log.UnitOfWork.BusCommandMessages.Single();
                var c100 = c100wrapped.Deserialise<C100Ping>();
                c100.PingedBy.Should().Be(nameof(P555TestUnitOfWork));

                var e150wrapped = log.UnitOfWork.BusEventMessages.Single();
                var e150 = e150wrapped.Deserialise<E150Pong>();
                e150.PongedBy.Should().Be(nameof(P555TestUnitOfWork));
            }
        }
    }
}