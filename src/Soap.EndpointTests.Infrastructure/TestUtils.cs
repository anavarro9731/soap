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
            public static Task<GetMessageLogItemQuery.ResponseModel> CommandCompletion(Guid messageId, int timeoutMsec = 45000, int pollIntervalMsec = 1000)
            {
                {
                    var messageResults = AwaitMessageResults().Result;

                    return Task.FromResult(messageResults);
                }

                Task<GetMessageLogItemQuery.ResponseModel> AwaitMessageResults()
                {
                    GetMessageLogItemQuery.ResponseModel queryResponse = null;

                    try
                    {
                        using (var cancellationTokenSource = new CancellationTokenSource(timeoutMsec))
                        {
                            var token = cancellationTokenSource.Token;
                            var listener = Task.Factory.StartNew(
                                () =>
                                    {
                                    var http = Endpoints.Http.CreateApiClient(typeof(GetMessageLogItemQuery).Assembly);

                                    while (true)
                                    {
                                        var queryRequest = new GetMessageLogItemQuery
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
                                        $"{nameof(GetMessageLogItemQuery)} has timed-out after {timeoutMsec} milliseconds for Message ID {messageId}");
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

                bool IsComplete(GetMessageLogItemQuery.ResponseModel messageLogItem)
                {
                    return messageLogItem?.SuccessfulAttempt != null || messageLogItem?.FailedAttempts.Count >= messageLogItem?.MaxFailedMessages;
                }
            }

            public static Task CommandFailure(Guid messageId, int timeoutMsec = 45000, int pollIntervalMsec = 1000)
            {
                var messageResults = CommandCompletion(messageId, timeoutMsec, pollIntervalMsec).Result;

                messageResults.Should()
                              .NotBeNull($"a {nameof(GetMessageLogItemQuery.ResponseModel)} is expected to exist for {nameof(ApiCommand.MessageId)} {messageId}");

                messageResults.SuccessfulAttempt.Should()
                              .BeNull(
                                  $"the {nameof(GetMessageLogItemQuery.ResponseModel)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to contain only failed attempt(s)");

                messageResults.FailedAttempts?.Count.Should()
                              .Be(
                                  messageResults.MaxFailedMessages,
                                  $"the {nameof(GetMessageLogItemQuery.ResponseModel)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to have failed the maximum number of times");

                return Task.CompletedTask;
            }

            public static Task CommandSuccess(Guid messageId, int timeoutMsec = 45000, int pollIntervalMsec = 1000)
            {
                var messageResults = CommandCompletion(messageId, timeoutMsec, pollIntervalMsec).Result;

                messageResults.Should()
                              .NotBeNull($"a {nameof(GetMessageLogItemQuery.ResponseModel)} is expected to exist for {nameof(ApiCommand.MessageId)} {messageId}");

                if (messageResults.FailedAttempts?.Count >= messageResults.MaxFailedMessages)
                {
                    var lastFailedAttempt = messageResults.FailedAttempts?.FirstOrDefault();
                    throw lastFailedAttempt.Error;
                }

                messageResults.SuccessfulAttempt.Should()
                              .NotBeNull(
                                  $"the {nameof(GetMessageLogItemQuery.ResponseModel)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to contain a successful attempt");

                return Task.CompletedTask;
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