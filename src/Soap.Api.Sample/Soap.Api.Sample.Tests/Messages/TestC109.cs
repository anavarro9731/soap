namespace Soap.Api.Sample.Tests.Messages
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC109 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC109(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(
                new C109v1_GetForm
                {
                    C109_FormDataEventName = typeof(E103v1_GetC107Form).FullName
                },
                Identities.UserOne,
                setupMocks: messageAggregatorForTesting =>
                    {
                    messageAggregatorForTesting.When<BlobStorage.Events.BlobGetSasTokenEvent>().Return("fake-token");
                    
                    }).Wait();
        }

        [Fact]
        public void ItShouldNotPublishAResponseToTheBus()
        {
            Result.MessageBus.BusEventsPublished.Should().BeEmpty();
        }

        [Fact]
        public void ItShouldPublishAResponseToTheWebSocketClient()
        {
            Result.MessageBus.WsEventsPublished.Should().ContainSingle();
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E103v1_GetC107Form>();
            (Result.MessageBus.WsEventsPublished.Single() as E103v1_GetC107Form).E000_CommandName.Should()
                                                                                .Be(
                                                                                    typeof(C107v1_CreateOrUpdateTestDataTypes)
                                                                                        .ToShortAssemblyTypeName());
        }
    }
}
