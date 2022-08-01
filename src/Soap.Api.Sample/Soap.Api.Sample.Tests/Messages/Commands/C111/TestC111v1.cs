//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C111
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Api.Sample.Models.ValueTypes;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC111v1 : Test
    {
        private static readonly Guid testDataId1 = Guid.NewGuid();

        private static readonly Guid testDataId2 = Guid.NewGuid();
        
        private static readonly Guid testDataId3 = Guid.NewGuid();

        public TestC111v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByAddingADatabaseEntry(
                new TestData
                {
                    Guid = testDataId1,
                    id = testDataId1,
                    Created = DateTime.UtcNow,
                    CreatedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToMillisecondsEpochTime(),
                    CustomObject = new Address()
                });

            SetupTestByAddingADatabaseEntry(
                new TestData
                {
                    Guid = testDataId2,
                    id = testDataId2,
                    Created = DateTime.UtcNow,
                    CreatedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToMillisecondsEpochTime(),
                    CustomObject = new Address()
                });
            
            SetupTestByAddingADatabaseEntry(
                new TestData
                {
                    Guid = testDataId3,
                    id = testDataId3,
                    Created = DateTime.UtcNow,
                    CreatedAsMillisecondsEpochTime = DateTime.UtcNow.ConvertToMillisecondsEpochTime(),
                    CustomObject = new Address()
                });

            TestMessage(new C111v1_GetRecentTestData(), Identities.JohnDoeAllPermissions).Wait();
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
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E105v1_GotRecentTestData>();
            (Result.MessageBus.WsEventsPublished.Single() as E105v1_GotRecentTestData).E105_TestData.Should().HaveCount(3);
            (Result.MessageBus.WsEventsPublished.Single() as E105v1_GotRecentTestData).E105_TestData[0]
                .E105_Id.Should()
                .Be(testDataId3); //* ordered descending
        }
    }
}
