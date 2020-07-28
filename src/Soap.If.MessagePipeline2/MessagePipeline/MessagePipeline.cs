namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.UnitOfWork;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public static class MessagePipeline
    {
        public static async Task Execute(string messageJson, string assemblyQualifiedName, Func<BoostrappedContext> getContext)
        {
            {
                (DateTime receivedTime, long receivedTicks) timeStamp = (DateTime.UtcNow, StopwatchOps.GetStopwatchTimestamp());

                DeserialiseMessage(
                    messageJson,
                    assemblyQualifiedName,
                    timeStamp,
                    getContext(),
                    out var contextAfterMessageObtained,
                    out var success);

                if (!success) return;

                ContextWithMessageLogEntry matureContext = null;
                try
                {
                    await PrepareContext(contextAfterMessageObtained, v => matureContext = v);
                }
                catch (Exception exception)
                {
                    contextAfterMessageObtained.Logger.Fatal(
                        "Cannot complete context preparation: partial context {@context}, error {@error}",
                        contextAfterMessageObtained,
                        exception);
                    return;
                }

                try
                {
                    /* THIS MUST BE SET AT THIS LEVEL. SETTING IT LOWER WILL LOSE THE VALUE WHEN THE
                     CALLSTACK POPS TO THIS LEVEL AND MEAN IT IS NOT AVAILABLE TO THE PROCESSMESSAGE CALL
                    Floating the context via method-injection is too cumbersome and DI requires parameterised
                    constructor which cannot be create via generics. */
                    ContextWithMessageLogEntry.Instance.Value = matureContext;

                    await ProcessMessage(matureContext);
                }
                catch (Exception exception)
                {
                    await HandleFailure(matureContext, exception);
                }
            }

            async Task PrepareContext(
                ContextWithMessage contextAfterMessageObtained,
                Action<ContextWithMessageLogEntry> setContext)
            {
                IApiIdentity identity = null;
                MessageLogEntry messageLogEntry = null;

                var msg = contextAfterMessageObtained.Message;

                msg.Authenticate(contextAfterMessageObtained, v => identity = v); //TODO awaitable after finished

                await contextAfterMessageObtained.CreateOrFindLogEntry(identity, v => messageLogEntry = v);

                var context = contextAfterMessageObtained.Upgrade(messageLogEntry);

                setContext(context);
            }

            void DeserialiseMessage(
                string messageJson,
                string assemblyQualifiedName,
                (DateTime receivedTime, long receivedTicks) timeStamp,
                BoostrappedContext bootstrappedContext,
                out ContextWithMessage contextAfterMessageObtained,
                out bool success)
            {
                try //* deserialise the message
                {
                    var msg = JsonConvert.DeserializeObject(messageJson, Type.GetType(assemblyQualifiedName)).As<ApiMessage>();
                    contextAfterMessageObtained = bootstrappedContext.Upgrade(msg, timeStamp);
                    success = true;
                }
                catch (Exception e)
                {
                    bootstrappedContext.Logger.Fatal(
                        "Cannot deserialise message: type {@type}, error {@error}, json {@json} stack {@stack}",
                        assemblyQualifiedName,
                        e.Message,
                        messageJson,
                        e.StackTrace);

                    contextAfterMessageObtained = null;
                    success = false;
                }
            }

            async Task ProcessMessage(ContextWithMessageLogEntry context)
            {
                var msg = context.Message;
                msg.ValidateOrThrow(context);

                var result = await context.AttemptToFinishAnUnfinishedUnitOfWork();

                switch (result)
                {
                    case UnitOfWork.State.AllComplete:
                        context.SerilogSuccess();
                        return;

                    case UnitOfWork.State.New:
                    case UnitOfWork.State.AllRolledBack:

                        switch (msg)
                        {
                            case MessageFailedAllRetries m:
                                await context.HandleFinalFailure(m);
                                break;
                            case ApiCommand c:
                                await context.Handle(c);
                                break;
                            case ApiEvent e:
                                await context.Handle(e);
                                break;
                        }

                        /* from this point on we can crash, throw, lose power, it won't matter all
                        will be continued when the message is next dequeued. Prior to this any errors
                        will fall into the catch block below and result in the message being retried
                        from the beginning but there will be no unit of work on the MessageLogEntry */
                        await context.CommitChanges();

                        context.SerilogSuccess();
                        break;
                }
            }

            async Task HandleFailure(ContextWithMessageLogEntry context, Exception exception)
            {
                Exception finalException;

                try //- log the message failure
                {
                    var exceptionMessages = new FormattedExceptionInfo(exception, context);

                    finalException = exceptionMessages.ToEnvironmentSpecificError();

                    await context.MarkFailureInMessageLog(exceptionMessages);

                    context.SerilogFailure(exceptionMessages);
                }
                catch (Exception exceptionHandlingException)
                {
                    try //- log the second phase error with some detail
                    {
                        var orignalExceptionPlusHandlingException = new ExceptionHandlingException(
                            new AggregateException(exceptionHandlingException, exception));

                        var exceptionMessages = new FormattedExceptionInfo(orignalExceptionPlusHandlingException, context);

                        finalException = exceptionMessages.ToEnvironmentSpecificError();

                        context.Logger.Fatal("Cannot write error to db message log {@details}", exceptionMessages);
                    }
                    catch (Exception lastChanceException) //- log a minimal error message of last resort
                    {
                        /* avoid use of PipelineExceptionMessages here as it theoretically could be the source of the error
                        * this goes raw to the caller so show don't show any exception details
                        * Serilog should swallow it's own internal errors so logging should be safe here
                        */

                        context.Logger.Fatal(
                            "Could not log error to db message log or seq using standard code, ignoring previous error and logging only the raw exception"
                            + "{@messageId} {@originalException} {@secondException} {@finalException}",
                            exception,
                            exceptionHandlingException,
                            lastChanceException);

                        var lastChanceExceptionMessageForCaller =
                            $"{FormattedExceptionInfo.CodePrefixes.EXWHEX}: {context.AppConfig.DefaultExceptionMessage}";

                        finalException = new Exception(lastChanceExceptionMessageForCaller);
                    }
                }

                throw finalException;
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