namespace Soap.Pf.EndpointTestsBase
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Queries;
    using Soap.Pf.ClientServerMessaging.Routing;
    using Soap.Pf.EndpointClients;

    public static class TestUtils
    {
        public static class Assert
        {
            public static Task<GetMessageLogItemQuery.ResponseModel> CommandCompletion(
                Guid messageId,
                int timeoutMsec = 15000,
                int pollIntervalMsec = 1000)
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
                                    var http = new HttpApiClient(new []
                                    {
                                        new MessageTypeToHttpEndpointRoute(typeof(GetMessageLogItemQuery), Query.ApiHostUri.OriginalString), 
                                    });

                                    while (true)
                                    {
                                        var queryRequest = new GetMessageLogItemQuery
                                        {
                                            MessageIdOfLogItem = messageId
                                        };

                                        queryResponse = http.SendQuery<GetMessageLogItemQuery.ResponseModel>(queryRequest).Result;

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

            public static Task CommandFailure(Guid messageId, int timeoutMsec = 15000, int pollIntervalMsec = 1000)
            {
                var messageResults = CommandCompletion(messageId, timeoutMsec, pollIntervalMsec).Result;

                messageResults.Should()
                              .NotBeNull(
                                  $"a {nameof(GetMessageLogItemQuery.ResponseModel)} is expected to exist for {nameof(ApiCommand.MessageId)} {messageId}");

                messageResults.SuccessfulAttempt.Should()
                              .BeNull(
                                  $"the {nameof(GetMessageLogItemQuery.ResponseModel)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to contain only failed attempt(s)");

                messageResults.FailedAttempts?.Count.Should()
                              .Be(
                                  messageResults.MaxFailedMessages,
                                  $"the {nameof(GetMessageLogItemQuery.ResponseModel)} for {nameof(ApiCommand.MessageId)} {messageId} was expected to have failed the maximum number of times");

                return Task.CompletedTask;
            }

            public static Task CommandSuccess(Guid messageId, int timeoutMsec = 15000, int pollIntervalMsec = 1000)
            {
                var messageResults = CommandCompletion(messageId, timeoutMsec, pollIntervalMsec).Result;

                messageResults.Should()
                              .NotBeNull(
                                  $"a {nameof(GetMessageLogItemQuery.ResponseModel)} is expected to exist for {nameof(ApiCommand.MessageId)} {messageId}");

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

        public static class Command
        {
            public const int LongRunningPollIntervalMsec = 20000;

            public const int LongRunningTimeoutMsec = 120000;

            public static string ApiHostEndpointAddress = ConfigurationManager.AppSettings["CommandToForward.ApiHostQueue"]
                                                                              ?.Replace("@localhost", $"@{Environment.MachineName}");
        }

        public static class Query
        {
            public static Uri ApiHostUri = new Uri(ConfigurationManager.AppSettings["Query.ApiHostUri"]);
        }
    }
}