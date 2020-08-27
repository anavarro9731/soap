namespace Sample.Tests.Messages
{
    using DataStore.Options;
    using FluentAssertions;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC104ShouldFailOnEtagButWithOptimisticConcurrencyOffItSucceeds : TestC104
    {
        public TestC104ShouldFailOnEtagButWithOptimisticConcurrencyOffItSucceeds(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        /* Run 1 message succeeds in spite of eTag violation
        */
        public async void ShouldFailOnEtagButWithOptimisticConcurrencyOffItSucceeds()
        {
            //act
            var c104TestUnitOfWork =
                Commands.TestUnitOfWork(SpecialIds.ShouldFailOnEtagButWithOptimisticConcurrencyOffItSucceeds);

            await TestMessage(
                c104TestUnitOfWork,
                Identities.UserOne,
                1,
                default,
                DataStoreOptions.Create().DisableOptimisticConcurrency());

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
    }
}