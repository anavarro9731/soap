namespace Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.MessagePipeline.Logging;
    using Xunit.Abstractions;

    public class TestC104 : Test
    {
        public TestC104(ITestOutputHelper output)
            : base(output)
        {
            //arrange
            Add(Aggregates.HanSolo);
            Add(Aggregates.DarthVader);
            Add(Aggregates.PrincessLeia);
            Add(Aggregates.LukeSkywalker);
        }

        protected void CountDataStoreOperationsSaved(MessageLogEntry log)
        {
            log.UnitOfWork.DataStoreCreateOperations.Count.Should().Be(2);
            log.UnitOfWork.DataStoreUpdateOperations.Count.Should().Be(3); //* includes soft delete
            log.UnitOfWork.DataStoreDeleteOperations.Count.Should().Be(1);
        }

        protected void CountMessagesSaved(MessageLogEntry log)
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