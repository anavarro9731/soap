namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.UnitOfWork;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class MessagePipeline
    {
        public static async Task Execute(string messageJson, string assemblyQualifiedName, Func<MContext.PerCallStageOneVariables> setCallVars)
        {
            {
                var receivedAtTick = StopwatchOps.GetStopwatchTimestamp();
                var receivedAt = DateTime.UtcNow;
                MessageLogEntry messageLogEntry = null;
                ApiMessage m;

                try
                {
                    m = JsonSerializer.Deserialize(messageJson, Type.GetType(assemblyQualifiedName)).As<ApiMessage>();
                }
                catch (Exception e)
                {
                    MContext.Logger.Fatal(
                        "Cannot deserialise message: type {@type}, error {@error}, json {@json}",
                        assemblyQualifiedName,
                        e.Message,
                        messageJson);
                    return;
                }

                try //- execute the message
                {
                    IIdentityWithPermissions identity = null;

                    await m.CreateOrFindLogEntry(v => messageLogEntry = v);

                    m.Authenticate(v => identity = v);

                    MContext.AfterMessageLogEntryObtained.UpdateContext(
                        m,
                        messageLogEntry,
                        (receivedAt, receivedAtTick),
                        identity);

                    m.ValidateOrThrow();

                    var result = await messageLogEntry.UnitOfWork.AttemptToFinishAPreviousAttempt(
                                     MContext.BusContext,
                                     MContext.DataStore);

                    switch (result)
                    {
                        case UnitOfWorkExtensions.State.AllComplete:
                            m.SerilogSuccess();
                            return;

                        case UnitOfWorkExtensions.State.New:
                        case UnitOfWorkExtensions.State.AllRolledBack:

                            switch (m)
                            {
                                case ApiCommand c when c.IsFailedAllRetriesMessage:
                                    m.HandleFinalFailure();
                                    break;
                                case IApiQuery q:
                                    var responseEvent = await q.Handle();
                                    MContext.BusContext.Publish(responseEvent);
                                    break;
                                case ApiCommand c:
                                    await c.Handle();
                                    break;
                                case ApiEvent e:
                                    await e.Handle();
                                    break;
                            }

                            /* from this point on we can crash, throw, lose power, it won't matter all
                            will be continued when the message is next dequeued. Prior to this any errors
                            will fall into the catch block below and result in the message being retried
                            from the beginning but there will be no unit of work on the MessageLogEntry */
                            await QueuedStateChanges.CommitChanges();

                            m.SerilogSuccess();
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Exception finalException;

                    try //- log the message failure
                    {
                        var exceptionMessages = new FormattedExceptionInfo(exception);

                        finalException = exceptionMessages.ToEnvironmentSpecificError();

                        await m.MarkFailureInMessageLog(exceptionMessages);

                        m.SerilogFailure(exceptionMessages);
                    }
                    catch (Exception exceptionHandlingException)
                    {
                        try //- log the second phase error with some detail
                        {
                            var orignalExceptionPlusHandlingException =
                                new ExceptionHandlingException(new AggregateException(exceptionHandlingException, exception));

                            var exceptionMessages = new FormattedExceptionInfo(orignalExceptionPlusHandlingException);

                            finalException = exceptionMessages.ToEnvironmentSpecificError();

                            MContext.Logger.Fatal("Cannot write error to db message log {@details}", exceptionMessages);
                        }
                        catch (Exception lastChanceException) //- log a minimal error message of last resort
                        {
                            /* avoid use of PipelineExceptionMessages here as it theoretically could be the source of the error
                            * this goes raw to the caller so show don't show any exception details
                            * Serilog should swallow it's own internal errors so logging should be safe here
                            */

                            MContext.Logger.Fatal(
                                "Could not log error to db message log or seq using standard code, ignoring previous error and logging only the raw exception"
                                + "{@messageId} {@originalException} {@secondException} {@finalException}",
                                exception,
                                exceptionHandlingException,
                                lastChanceException);

                            var lastChanceExceptionMessageForCaller =
                                $"{FormattedExceptionInfo.CodePrefixes.EXWHEX}: {MContext.AppConfig.DefaultExceptionMessage}";

                            finalException = new Exception(lastChanceExceptionMessageForCaller);
                        }
                    }

                    throw finalException;
                }
            }
        }

        public static class Constants
        {
            public static readonly Guid ForceFailBeforeMessageCompletesAndFailErrorHandlerId =
                Guid.Parse("c7c607b3-fc47-4dbd-81da-8ef28c785a2f");

            public static readonly Guid ForceFailBeforeMessageCompletesId = Guid.Parse("006717f1-fe50-4d0b-b762-75b883ba4a65");
        }
    }
}