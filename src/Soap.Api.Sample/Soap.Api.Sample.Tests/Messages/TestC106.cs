namespace Soap.Api.Sample.Tests.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC106 : Test
    {
        Guid id = Guid.NewGuid();
        
        public TestC106(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

            void Set(C106LargeCommand c)
            {
                c.Headers.SetMessageId(id);
                c.Headers.SetBlobId(id);
            }

            var c106FromBlobStorage = new C106LargeCommand().Op(Set).Op(c => c.Headers.SetDefaultHeadersForIncomingTestMessages(c));
            
            var c106In = new C106LargeCommand().Op(
                c =>
                    {
                    Set(c);
                    c.Large256KbString = null;
                    });

             SetupTestByProcessingAMessage(
                c106In, 
                Identities.UserOne,
                setup: messageAggregatorForTesting => messageAggregatorForTesting.When<BlobStorage.Events.BlobDownloadEvent>()
                                                                                 .Return(Task.FromResult(c106FromBlobStorage.ToBlob())));
        }

        [Fact]
        public async void ItShouldNotFailToRetrieveTheValuesFromBlobStorage()
        {
            var entry = await Result.DataStore.ReadById<MessageLogEntry>(id);
            entry.SerialisedMessage.Deserialise<C106LargeCommand>().Large256KbString.Should().NotBeNullOrEmpty();
        }
    }
}