namespace Soap.Pf.EndpointClients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;

    public class HttpApiClient : IDisposable
    {
        private readonly HttpClient http = new HttpClient();

        private readonly IEnumerable<HttpMessageRoute> httpRoutes;

        public HttpApiClient(IEnumerable<HttpMessageRoute> httpRoutes, TimeSpan? httpRequestTimeout = null)
        {
            this.httpRoutes = httpRoutes;

            ConfigureHttpClient();

            void ConfigureHttpClient()
            {
                this.http.DefaultRequestHeaders.Accept.Clear();
                this.http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.http.Timeout = httpRequestTimeout ?? this.http.Timeout;
            }
        }

        public void Dispose()
        {
            this.http?.Dispose();
        }

        // Forward to BusClient CommandToForward
        public async Task SendCommand(IApiCommand command)
        {
            if (command is IForwardCommandFromHttpToMsmq alreadyWrappedCommand)
            {
                await SendAndWait<object>(GetEndpointUri(alreadyWrappedCommand.CommandToForward), command);
            }
            else
            {
                //.. set message id if its not set
                if (command.MessageId == Guid.Empty) command.MessageId = Guid.NewGuid();

                //.. create the wrapper type
                var closedGenericType = typeof(ForwardCommandFromHttpToMsmq<>).MakeGenericType(command.GetType());
                //.. instantiate it with the message to forward
                var newWrappedCommand = (IApiCommand)Activator.CreateInstance(closedGenericType, command);

                await SendAndWait<object>(GetEndpointUri(command), newWrappedCommand).ConfigureAwait(false);
            }
        }

        // Request / Response CommandToForward With Static Return Type
        public async Task<TResponse> SendCommand<TResponse>(ApiCommand<TResponse> command) where TResponse : class, new()
        {
            return await SendAndWait<TResponse>(GetEndpointUri(command), command).ConfigureAwait(false);
        }

        // Request / Response CommandToForward With Dynamic Return Type
        public async Task<dynamic> SendCommand(ApiCommand command)
        {
            return await SendAndWait<dynamic>(GetEndpointUri(command), command).ConfigureAwait(false);
        }

        /* Request+Result (success/failed result)
         * TODO 
         */

        // Query With Static Return Type
        public async Task<TResponse> SendQuery<TResponse>(ApiQuery<TResponse> query) where TResponse : class, new()
        {
            return await SendAndWait<TResponse>(GetEndpointUri(query), query).ConfigureAwait(false);
        }

        // Query Without Dynamic Return Type
        public async Task<dynamic> SendQuery(ApiQuery query)
        {
            return await SendAndWait<dynamic>(GetEndpointUri(query), query).ConfigureAwait(false);
        }

        private Uri GetEndpointUri(IApiMessage message)
        {
            var route = this.httpRoutes.FirstOrDefault(r => r.CanRouteMessage(message))?.EndpointAddressHttp.EndpointUri;

            Guard.Against(route == null, $"No route exists for message {message.GetType().AssemblyQualifiedName}");

            return route;
        }

        private async Task<TResponse>
            SendAndWait<TResponse>(Uri requestUri, IApiMessage message)
        {
            if (message.MessageId == Guid.Empty) message.MessageId = Guid.NewGuid();

            if (message.TimeOfCreationAtOrigin.HasValue == false) message.TimeOfCreationAtOrigin = DateTime.UtcNow;

            var json = message.ToJson();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var uriBuilder = new UriBuilder(requestUri)
            {
                Path = message.CanChangeState() ? "/command" : "/query"
            };

            requestUri = uriBuilder.Uri;

            var responseMessage = await this.http.PostAsync(requestUri, content).ConfigureAwait(false);

            var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);


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

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"The server returned status code: {((int)responseMessage.StatusCode).ToString()} {responseMessage.StatusCode.ToString()}");
            }


            var responseObject = responseContent.FromJson<TResponse>();

            return responseObject;
        }

        public class ErrorHttpResponse
        {
            public string Error { get; set; }
        }
    }
}