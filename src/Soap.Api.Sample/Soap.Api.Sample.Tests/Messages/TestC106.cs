//##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC106 : Test
    {
        private readonly Guid id = Guid.NewGuid();

        public TestC106(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            void SetHeaders(C106v1_LargeCommand c)
            {
                c.Headers.SetMessageId(this.id);
                c.Headers.SetBlobId(this.id);
            }

            var c106FromBlobStorage = new C106v1_LargeCommand().Op(SetHeaders);

            var c106In = new C106v1_LargeCommand().Op(
                c =>
                    {
                    SetHeaders(c);
                    c.C106_Large256KbString = null;
                    });

            TestMessage(
                c106In,
                Identities.UserOne,
                0,
                setupMocks: messageAggregatorForTesting => messageAggregatorForTesting.When<BlobStorage.Events.BlobDownloadEvent>()
                                                                                 .Return(
                                                                                     Task.FromResult(
                                                                                         c106FromBlobStorage.ToBlob())));
        }

        [Fact]
        public async void ItShouldNotFailToRetrieveTheValuesFromBlobStorage()
        {
            var entry = await Result.DataStore.ReadById<MessageLogEntry>(this.id);
            entry.SerialisedMessage.Deserialise<C106v1_LargeCommand>().C106_Large256KbString.Should().NotBeNullOrEmpty();
        }
    }
}
