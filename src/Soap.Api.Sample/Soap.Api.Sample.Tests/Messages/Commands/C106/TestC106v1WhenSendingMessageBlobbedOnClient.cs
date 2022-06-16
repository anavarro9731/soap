//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C106
{
    using System;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC106v1WhenSendingMessageBlobbedOnClient : Test
    {
        private readonly Guid id = Guid.NewGuid();

        public TestC106v1WhenSendingMessageBlobbedOnClient(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            void SetHeaders(C106v1_LargeCommand c)
            {
                c.Headers.SetMessageId(this.id);
            }

            var c106V1LargeCommand = new C106v1_LargeCommand().Op(SetHeaders);

            TestMessage(c106V1LargeCommand, Identities.JohnDoeAllPermissions, 0).Wait();
        }

        [Fact]
        public async void TheBlobStorageEntryShouldBeComplete()
        {
            (await Result.BlobStorage.GetBlobOrNull(this.id, "large-messages")).ToMessage()
                                                                               .As<C106v1_LargeCommand>()
                                                                               .C106_Large256KbString.Should()
                                                                               .NotBeNullOrWhiteSpace();
        }
        
        [Fact]
        public void TheMessageLogEntryShouldBeASkeleton()
        {
            Result.GetMessageLogEntry().SkeletonOnly.Should().BeTrue();
            Result.GetMessageLogEntry().SerialisedMessage.Deserialise<C106v1_LargeCommand>().C106_Large256KbString.Should().BeNullOrWhiteSpace();
        }
    }
}