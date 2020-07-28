﻿namespace Soap.Pf.EndpointTestsBase
{
    public class AbstractWhenPingingTheServiceViaMsmq<TPing, TPingResponse, TGetMessageLogItem, TGetMessageLogItemResponse>
        where TPing : AbstractPingCommandForMsmq<TPingResponse>, new()
        where TPingResponse : class, IApiCommand, new()
        where TGetMessageLogItem : AbstractGetMessageLogItemQuery<TGetMessageLogItemResponse>, new()
        where TGetMessageLogItemResponse : AbstractGetMessageLogItemQuery<TGetMessageLogItemResponse>.AbstractResponseModel, new()
    {
        private readonly Guid pingCommandId = Guid.NewGuid();

        private readonly string testName = "WhenPingingTheServiceViaMsmq";

        [Fact]
        public async Task ShouldPublishAPongEvent()
        {
            //TODO
            await Setup();
        }

        [Fact]
        public async Task ShouldReplyWithAPongCommand()
        {
            //TODO
            await Setup();
        }

        private async Task Setup()
        {
            //arrange
            var apiClient = TestUtils.Endpoints.Msmq.CreateApiClient(typeof(TPing).Assembly);

            var message = new TPing().Op(p => p.PingedBy = this.testName);

            //act
            await apiClient.Send(message);
        }
    }
}