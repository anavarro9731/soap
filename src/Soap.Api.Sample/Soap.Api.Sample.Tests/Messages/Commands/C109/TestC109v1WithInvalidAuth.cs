namespace Soap.Api.Sample.Tests.Messages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC109v1InvalidAuth : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC109v1InvalidAuth(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(
                new C109v1_GetForm
                {
                    C109_FormDataEventName = typeof(E103v1_GetC107Form).FullName
                },
                Identities.JaneDoeNoPermissions,
                setupMocks: messageAggregatorForTesting =>
                    {
                    messageAggregatorForTesting.When<BlobStorage.Events.BlobGetSasTokenEvent>().Return("fake-token");
                    
                    }).Wait();
        }

        [Fact]
        public void ItShouldThrowPermissionsError()
        {
            Result.Success.Should().BeFalse();
            Result.UnhandledError.Should().BeOfType<DomainExceptionWithErrorCode>();
            (Result.UnhandledError as DomainExceptionWithErrorCode).Error.Should()
                                                                   .Be(AuthErrorCodes.NoApiPermissionExistsForThisMessage);
        }

    }
}
