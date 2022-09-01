//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C114
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC114v1 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC114v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByAddingADatabaseEntry(
                new TestData
                {
                    id = testDataId,
                    Decimal = 44.44M
                });

            TestMessage(
                    new C114v1_DeleteTestDataById
                    {
                        C114_TestDataId = testDataId
                    },
                    Identities.JohnDoeAllPermissions)
                .Wait();
        }

        [Fact]
        public async void ItShouldDeleteTheEntry()
        {
            //* john doe has hard delete permissions
            (await Result.DataStore.ReadById<TestData>(testDataId)).Should().BeNull();
        }
        
        [Fact]
        public void ItShouldPublishAResponseToTheBus()
        {
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.BusEventsPublished.Single().Should().BeOfType<E106v1_TestDataRemoved>();
            Result.MessageBus.BusEventsPublished.Single().As<E106v1_TestDataRemoved>().E106_TestDataId.Should().Be(testDataId);
        }

        [Fact]
        public void ItShouldPublishAResponseToTheWebSocketClient()
        {
            Result.MessageBus.WsEventsPublished.Should().ContainSingle();
            Result.MessageBus.WsEventsPublished.Single().Should().BeOfType<E106v1_TestDataRemoved>();
            Result.MessageBus.WsEventsPublished.Single().As<E106v1_TestDataRemoved>().E106_TestDataId.Should().Be(testDataId);
        }
        
        [Fact]
        public void ItShouldNotSendToAQueue()
        {
            Result.MessageBus.BusEventsSentDirectToQueue.Should().BeEmpty();
        }
    }
}
