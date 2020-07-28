namespace Soap.Pf.EndpointTestsBase
{
    public class AbstractWhenPingingTheServiceViaHttp<TPing, TPingResponse, TGetMessageLogItem, TGetMessageLogItemResponse>
        where TPing : AbstractPingCommandForHttp<TPingResponse>, new()
        where TPingResponse : AbstractPingCommandForHttp<TPingResponse>.AbstractResponseModel, new()
        where TGetMessageLogItem : AbstractGetMessageLogItemQuery<TGetMessageLogItemResponse>, new()
        where TGetMessageLogItemResponse : AbstractGetMessageLogItemQuery<TGetMessageLogItemResponse>.AbstractResponseModel, new()
    {
        private readonly Guid pingCommandId = Guid.NewGuid();

        private readonly string testName = "WhenPingingTheServiceViaHttp";

        [Fact]
        public async Task ShouldPublishAPongEvent()
        {
            //TODO
            await Setup();
        }

        [Fact]
        public async Task ShouldQueueAPongCommand()
        {
            //TODO
            await Setup();
        }

        [Fact]
        public async Task ShouldReturnAPingViewModel()
        {
            await Setup();

            var response =
                await TestUtils.Assert.CommandSuccess<TGetMessageLogItem, TGetMessageLogItemResponse>(this.pingCommandId);

            var typedResponse = response.SuccessfulAttempt.ReturnValue as JObject;

            typedResponse.ToObject<TPingResponse>().PingedBy.Should().Be(this.testName);
        }

        private async Task Setup()
        {
            //arrange
            var apiClient = TestUtils.Endpoints.Http.CreateApiClient(typeof(TPing).Assembly);

            var message = new TPing().Op(
                p =>
                    {
                    p.PingedBy = this.testName;
                    p.MessageId = this.pingCommandId;
                    });

            //act
            await apiClient.SendCommand(message);
        }
    }
}