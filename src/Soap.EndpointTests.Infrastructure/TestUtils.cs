namespace Soap.Pf.EndpointTestsBase
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.ClientServerMessaging.Queries;
    using Soap.Pf.ClientServerMessaging.Routing.Routes;
    using Soap.Pf.EndpointClients;

    public static class TestUtils
    {
        public static class Assert
        {
            public static Task<TMessageLogItemQueryResponse> CommandCompletion<TMessageLogItemQuery, TMessageLogItemQueryResponse>(Guid messageId, int timeoutMsec = 45000, int pollIntervalMsec = 1000)

                where TMessageLogItemQuery : AbstractGetMessageLogItemQuery<TMessageLogItemQueryResponse>, new()
                where TMessageLogItemQueryResponse : AbstractGetMessageLogItemQuery<TMessageLogItemQueryResponse>.AbstractResponseModel, new()

            {
                {
                    var messageResults = AwaitMessageResults().Result;

                    return Task.FromResult(messageResults);
                }

                Task<TMessageLogItemQueryResponse> AwaitMessageResults()
                {
                    TMessageLogItemQueryResponse queryResponse = null;

                    try
                    {
                        using (var cancellationTokenSource = new CancellationTokenSource(timeoutMsec))
                        {
                            var token = cancellationTokenSource.Token;
                            var listener = Task.Factory.StartNew(
                                () =>
                                    {
                                    var http = Endpoints.Http.CreateApiClient(typeof(TMessageLogItemQuery).Assembly);

                                    while (true)
                                    {
                                        var queryRequest = new TMessageLogItemQuery
                                        {
                                            MessageIdOfLogItem = messageId
                                        };

                                        queryResponse = http.SendQuery(queryRequest).Result;

                                        if (IsComplete(queryResponse)) break;

                                        if (token.IsCancellationRequested) break;

                                        Task.Delay(TimeSpan.FromMilliseconds(pollIntervalMsec), token).Wait();

                                        if (token.IsCancellationRequested) break;
                                    }
                                    },
                                token,
                                TaskCreationOptions.LongRunning,
                                TaskScheduler.Default);

                            var timedOut = listener.Wait(TimeSpan.FromMilliseconds(timeoutMsec)) == false;

                            if (timedOut || cancellationTokenSource.IsCancellationRequested)
                            {
                                if (queryResponse == null)
                                {
                                    throw new TimeoutException(
                                        $"{nameof(TMessageLogItemQuery)} has timed-out after {timeoutMsec} milliseconds for Message ID {messageId}");
                                }
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (AggregateException aex)
                    {
                        if (aex.Flatten().InnerExceptions.Any(ex => ex is TaskCanceledException == false)) throw;
                    }

                    return Task.FromResult(queryResponse);
                }

                bool IsComplete(TMessageLogItemQueryResponse messageLogItem)
                {
                    return messageLogItem?.SuccessfulAttempt != null || messageLogItem?.FailedAttempts.Count >= messageLogItem?.MaxFailedMessages;
                }
            }

            public static Task<TMessageLogItemQueryResponse> CommandFailure<TMessageLogItemQuery, TMessageLogItemQueryResponse>(Guid messageId, int timeoutMsec = 45000, int pollIntervalMsec = 1000)
                where TMessageLogItemQuery : AbstractGetMessageLogItemQuery<TMessageLogItemQueryResponse>, new()
                where TMessageLogItemQueryResponse : AbstractGetMessageLogItemQuery<TMessageLogItemQueryResponse>.AbstractResponseModel, new()
            {
                var messageResults = CommandCompletion<TMessageLogItemQuery, TMessageLogItemQueryResponse>(messageId, timeoutMsec, pollIntervalMsec).Result;

                messageResults.Should()
                              .NotBeNull($"a {nameof(TMessageLogItemQueryResponse)} is expected to exist for {nameof(ApiCommand.MessageId)} {messageId}");

                messageResults.SuccessfulAttempt.Should()
                              .BeNull(
                                  $"the {nameof(TMessageLogItemQueryResponse)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to contain only failed attempt(s)");

                messageResults.FailedAttempts?.Count.Should()
                              .Be(
                                  messageResults.MaxFailedMessages,
                                  $"the {nameof(TMessageLogItemQueryResponse)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to have failed the maximum number of times");

                return Task.FromResult(messageResults);
            }

            public static Task<TMessageLogItemQueryResponse> CommandSuccess<TMessageLogItemQuery, TMessageLogItemQueryResponse>(Guid messageId, int timeoutMsec = 45000, int pollIntervalMsec = 1000)
                where TMessageLogItemQuery : AbstractGetMessageLogItemQuery<TMessageLogItemQueryResponse>, new()
                where TMessageLogItemQueryResponse : AbstractGetMessageLogItemQuery<TMessageLogItemQueryResponse>.AbstractResponseModel, new()

            {
                var messageResults = CommandCompletion<TMessageLogItemQuery, TMessageLogItemQueryResponse>(messageId, timeoutMsec, pollIntervalMsec).Result;

                messageResults.Should()
                              .NotBeNull($"a {nameof(TMessageLogItemQueryResponse)} is expected to exist for {nameof(ApiCommand.MessageId)} {messageId}");

                if (messageResults.FailedAttempts?.Count >= messageResults.MaxFailedMessages)
                {
                    var lastFailedAttempt = messageResults.FailedAttempts?.FirstOrDefault();
                    throw lastFailedAttempt.Error;
                }

                messageResults.SuccessfulAttempt.Should()
                              .NotBeNull(
                                  $"the {nameof(TMessageLogItemQueryResponse)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to contain a successful attempt");

                return Task.FromResult(messageResults);
            }
        }

        public static class Endpoints
        {
            public static class Http
            {
                private static readonly Uri ApiHostUri = new Uri(ConfigurationManager.AppSettings["ApiHostUri"]);

                public static HttpApiClient CreateApiClient(Assembly assembly)
                {
                    return new HttpApiClient(
                        new[]
                        {
                            new MessageAssemblyToHttpEndpointRoute(assembly, ApiHostUri.OriginalString)
                        });
                }
            }

            public static class Msmq
            {
                private const int LongRunningPollIntervalMsec = 20000;

                private const int LongRunningTimeoutMsec = 120000;

                private static readonly string ApiHostEndpointAddress =
                    ConfigurationManager.AppSettings["ApiHostQueue"]?.Replace("@localhost", $"@{Environment.MachineName}");

                public static BusApiClient CreateApiClient(Assembly messageAssembly)
                {
                    var client = new BusApiClient(new MessageAssemblyToMsmqEndpointRoute(messageAssembly, ApiHostEndpointAddress)).Op(async c => await c.Start());
                    return client;
                }
            }
        }
    }
}