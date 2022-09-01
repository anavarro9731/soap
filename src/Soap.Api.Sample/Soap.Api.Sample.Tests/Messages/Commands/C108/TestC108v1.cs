//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C108
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC108v1 : Test
    {
        private static readonly Guid testDataId = Guid.NewGuid();

        public TestC108v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {


        }
        
        [Fact]
        public void WithQueueHeaderOnly()
        {
            TestMessage(
                    new C108v1_TestEventPublishedToQueue()
                    {
                        C108_SetQueueHeaderOnly = true  
                    },
                    Identities.JohnDoeAllPermissions)
                .Wait();
            
            //* this will send the direct event IN ADDITION to the others methods of transmission
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.WsEventsPublished.Should().ContainSingle(); //because test events have session headers
            Result.MessageBus.BusEventsSentDirectToQueue.Should().ContainSingle();
            Result.MessageBus.CommandsSent.Should().BeEmpty();
        }

        [Fact]
        public void ByUsingFlagOnly()
        {
            TestMessage(
                    new C108v1_TestEventPublishedToQueue()
                    {
                        C108_SetQueueHeaderOnly = false
                    },
                    Identities.JohnDoeAllPermissions)
                .Wait();
            
            //* this will send only by the queue
            Result.MessageBus.BusEventsPublished.Should().BeEmpty();
            Result.MessageBus.WsEventsPublished.Should().BeEmpty();
            Result.MessageBus.BusEventsSentDirectToQueue.Should().ContainSingle();
            Result.MessageBus.CommandsSent.Should().BeEmpty();
        }

    }
}
