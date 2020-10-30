namespace Soap.MessagePipeline
{
    using System;
    using System.Threading.Tasks;
    using Soap.Context.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Models;
    using Guard = DataStore.Models.PureFunctions.Guard;

    public static class MessagePipeline
    {
        public static async Task Execute(ApiMessage message, BoostrappedContext bootstrappedContext)
        {
            {

                await FillMessageFromStorageIfApplicable();
                
                ContextWithMessageLogEntry matureContext = null;
                
                await PrepareContext(bootstrappedContext, v => matureContext = v);
                
                try
                {
                    /* THIS MUST BE SET AT THIS LEVEL. SETTING IT LOWER (via say a method on the context called from
                     this level) WILL LOSE THE VALUE WHEN THE CALLSTACK POPS TO THIS LEVEL AND MEAN IT IS NOT AVAILABLE TO THE PROCESSMESSAGE CALL
                     It has to be set DIRECTLY from this level. Floating the context via method-injection is too cumbersome and 
                     DI requires a parameterised constructor which cannot be create via generics. */
                    ContextWithMessageLogEntry.Instance.Value = matureContext;

                    await ProcessMessage(matureContext);
                }
                catch (Exception exception)
                {
                    await HandleFailure(matureContext, exception);
                }
            }

            async Task PrepareContext(
                BoostrappedContext boostrappedContext,
                Action<ContextWithMessageLogEntry> setContext)
            {
                IApiIdentity identity = null;
                MessageLogEntry messageLogEntry = null;

                try
                {
                    (DateTime receivedTime, long receivedTicks) timeStamp = (DateTime.UtcNow, StopwatchOps.GetStopwatchTimestamp());

                    var contextAfterMessageObtained = boostrappedContext.Upgrade(message, timeStamp);

                    var msg = contextAfterMessageObtained.Message;  //* i think the only reason we are getting it from the context is in case it changes during upgrade() but at present it doesn't

                    msg.Authenticate(contextAfterMessageObtained, v => identity = v); // TODO awaitable after finished

                    await contextAfterMessageObtained.CreateOrFindLogEntry(identity, v => messageLogEntry = v);
                    
                    var context = contextAfterMessageObtained.Upgrade(messageLogEntry);
                    
                    setContext(context);
                }
                catch (Exception e)
                {
                    Guard.Against(true, $"Cannot complete context preparation: error {e}");
                }
            }

            async Task FillMessageFromStorageIfApplicable()
            {
                var blobId = message.Headers.GetBlobId();
                if (blobId != Guid.Empty)
                {
                    message = await bootstrappedContext.BlobStorage.GetApiMessageFromBlob(blobId);
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
                    case UnitOfWork.State.AllRolledBack:
                        //* don't try again after a rollback as you may be out of retries to rollback again
                        throw new DomainExceptionWithErrorCode(GlobalErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);

                    case UnitOfWork.State.New:

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
                        var originalExceptionPlusHandlingException = new ExceptionHandlingException(
                            new AggregateException(exceptionHandlingException, exception));

                        var exceptionMessages = new FormattedExceptionInfo(originalExceptionPlusHandlingException, context);

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
    }
}