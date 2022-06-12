//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C106
{
    using System;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Client;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    
    
    public class TestC106v1WhenSendingMessageLessThan256KUsingHttpDirect : Test
    {
        private readonly Guid id = Guid.NewGuid();

        public TestC106v1WhenSendingMessageLessThan256KUsingHttpDirect(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            void SetHeaders(C106v1_LargeCommand c)
            {
                c.Headers.SetMessageId(this.id);
            }

            var c106V1LargeCommand = new C106v1_LargeCommand().Op(SetHeaders).Op(x => x.C106_Large256KbString = "small string instead");
            
            TestMessage(c106V1LargeCommand, Identities.JohnDoeAllPermissions, 0, clientTransport: Transport.HttpDirect).Wait();
        }

        [Fact]
        public async void ThereShouldBeNoBlobStorageEntry()
        {
            (await Result.BlobStorage.GetBlobOrNull(this.id, "large-messages")).Should().BeNull();
        }
        
        [Fact]
        public void TheMessageLogEntryShouldBeTheFullMessage()
        {
            Result.GetMessageLogEntry().SkeletonOnly.Should().BeFalse();
            Result.GetMessageLogEntry().SerialisedMessage.Deserialise<C106v1_LargeCommand>().C106_Large256KbString.Should().Be("small string instead");
        }
    }
}