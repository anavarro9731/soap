namespace Soap.Endpoint.Clients
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public class HttpApiClient
    {
        private readonly string commandPath;

        private readonly HttpClient http = new HttpClient();

        private readonly string queryPath;

        public HttpApiClient(Uri apiEndpointBaseAddress, string commandPath = "command", string queryPath = "query", TimeSpan? httpRequestTimeout = null)
        {
            this.commandPath = commandPath;
            this.queryPath = queryPath;
            this.http.BaseAddress = apiEndpointBaseAddress;
            this.http.DefaultRequestHeaders.Accept.Clear();
            this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.http.Timeout = httpRequestTimeout ?? this.http.Timeout;
        }

        public async Task SendCommand(IApiCommand command)
        {
            IApiCommand wrappedMessage;
            if (command.GetType().IsSubclassOfRawGeneric(typeof(ForwardCommandToQueue<>)))
            {
                wrappedMessage = command;
            }
            else
            {
                if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

                var closedGenericType = typeof(ForwardCommandToQueue<>).MakeGenericType(command.GetType());
                wrappedMessage = (IApiCommand)Activator.CreateInstance(closedGenericType, command);
            }

            await SendCommand<object>(wrappedMessage).ConfigureAwait(false);
        }

        public async Task<TResponse> SendCommand<TResponse>(IApiCommand command)
        {
            return await SendAndWait<TResponse>(this.commandPath, command).ConfigureAwait(false);
        }

        public async Task<TResponse> SendQuery<TResponse>(IApiQuery query)
        {
            return await SendAndWait<TResponse>(this.queryPath, query).ConfigureAwait(false);
        }

        private async Task<TResponse> SendAndWait<TResponse>(string requestUri, IApiMessage message)
        {
            if (message.MessageId == Guid.Empty) message.MessageId = Guid.NewGuid();

            if (message.TimeOfCreationAtOrigin.HasValue == false) message.TimeOfCreationAtOrigin = DateTime.UtcNow;

            var json = message.ToJson();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var responseMessage = await this.http.PostAsync(requestUri, content).ConfigureAwait(false);

            var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            {
                // TResponse could be of type object/dynamic, so let's check that the response is NOT a failure!!!
                ErrorHttpResponse serverSideExceptionMessage = null;
                try
                {
                    serverSideExceptionMessage = responseContent.FromJson<ErrorHttpResponse>();
                }
                catch
                {
                    // Good!!! - ignore
                }
                if (serverSideExceptionMessage?.Error != null)
                {
                    throw new Exception("An error occurred on the server processing this message", new Exception(serverSideExceptionMessage.Error));
                }
            }

            var responseObject = responseContent.FromJson<TResponse>();

            return responseObject;
        }
    }
}