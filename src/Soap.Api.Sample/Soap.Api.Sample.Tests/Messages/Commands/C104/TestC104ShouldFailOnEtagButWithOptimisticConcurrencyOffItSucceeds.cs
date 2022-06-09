//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C104
{
    using DataStore.Options;
    using FluentAssertions;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces.Messages;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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
                Identities.JohnDoeAllPermissions,
                1,
                default,
                DataStoreOptions.Create().DisableOptimisticConcurrency());

            //assert
            var uow = Result.GetUnitOfWork();
            
            CountDataStoreOperationsSaved(uow);
            CountMessagesSaved(uow);
            CountMessagesSent();
        }

        private void CountMessagesSent()
        {
            Result.MessageBus.CommandsSent.Count.Should().Be(1);
            Result.MessageBus.BusEventsPublished.Count.Should().Be(1);
        }
    }
}
