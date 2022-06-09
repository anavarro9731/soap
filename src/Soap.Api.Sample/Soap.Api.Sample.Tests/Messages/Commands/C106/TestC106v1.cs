//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages.Commands.C106
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

    public class TestC106v1 : Test
    {
        private readonly Guid id = Guid.NewGuid();

        public TestC106v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            void SetHeaders(C106v1_LargeCommand c)
            {
                c.Headers.SetMessageId(this.id);
                c.Headers.SetBlobId(this.id);
            }

            var c106FromBlobStorage = new C106v1_LargeCommand().Op(SetHeaders);
            
            SetupTestByAddingABlobStorageEntry(c106FromBlobStorage.ToBlob(), CommonContainerNames.LargeMessages);

            var c106In = new C106v1_LargeCommand().Op(
                c =>
                    {
                    SetHeaders(c);
                    c.C106_Large256KbString = null;
                    });

            TestMessage(
                c106In,
                Identities.JohnDoeAllPermissions,
                0).Wait();
        }
        

        [Fact]
        public async void ItShouldNotFailToRetrieveTheValuesFromBlobStorage()
        {
            var entry = await Result.DataStore.ReadById<MessageLogEntry>(this.id);
            entry.SerialisedMessage.Deserialise<C106v1_LargeCommand>().C106_Large256KbString.Should().NotBeNullOrEmpty();
        }
    }
}
