//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C110
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Api.Sample.Models.ValueTypes;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC110v1 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC110v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByAddingADatabaseEntry(new TestData()
            {
                Guid = testDataId,
                id = testDataId,
                CustomObject = new Address()
            });
            
            TestMessage(
                new C110v1_GetTestDataById()
                {
                    C110_TestDataId = testDataId
                },
                Identities.JohnDoeAllPermissions).Wait();
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
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E102v1_GotTestDatum>();
            (Result.MessageBus.WsEventsPublished.Single() as E102v1_GotTestDatum).E102_TestData.E102_Guid.Should()
                                                                                .Be(testDataId);
        }
    }
}
