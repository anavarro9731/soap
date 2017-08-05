﻿namespace Palmtree.Sample.Api.Endpoint.Tests.Rebus.Commands
{
    using System;
    using Palmtree.ApiPlatform.Endpoint.Clients;
    using Palmtree.ApiPlatform.EndpointTests.Infrastructure;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Xunit;

    public class WhenSendingInvalidPingCommand
    {
        private readonly Guid pingCommandId = Guid.NewGuid();

        private string password;

        private Guid userId;

        public WhenSendingInvalidPingCommand()
        {
            var apiClient = new RebusApiClient("serviceapi");

            this.userId = Guid.NewGuid();
            this.password = "password";

            apiClient.SendCommand(
                         new PingCommand("OtherSpecialInvalidParamSeeCodeInHandler")
                         {
                             MessageId = this.pingCommandId
                         })
                     .GetAwaiter()
                     .GetResult();
        }
        /*
        [Fact]
        public void ItShouldHaveProcessedAMesageFailedAllRetriesMessage()
        {
            Thread.Sleep(1000);
            var queryClient = new HttpApiClient(new Uri("http://localhost:5055"));
            var messageFailedAllRetriesLogItem = queryClient.SendQuery<MessageFailedAllRetriesLogItem>(new GetMessageFailedAllRetriesLogItem(this.pingCommandId))
                                                            .Result;
            Assert.NotNull(messageFailedAllRetriesLogItem);
            Assert.Equal(messageFailedAllRetriesLogItem.IdOfMessageThatFailed, this.pingCommandId);
        }
        */

        [Fact]
        public void ItShouldRecordAllFailures()
        {
            Console.WriteLine("Checking message status");

            var messageLogItem = TestUtils.Assert.CommandCompletion(this.pingCommandId, 20000).Result;

            Console.WriteLine(
                $"Message Log Item: Max Failed Messages = {messageLogItem.MaxFailedMessages}, Failed Attemp Count = {messageLogItem.FailedAttempts.Count}");

            Assert.False(messageLogItem.MaxFailedMessages == 1, "This test is inconclusive when no retries were performed");

            Assert.Equal(messageLogItem.MaxFailedMessages, messageLogItem.FailedAttempts.Count);
        }

        //TODO: assert you cant create an infintite loop
    }
}
