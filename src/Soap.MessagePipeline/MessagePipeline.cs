namespace Soap.MessagePipeline
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Context.Exceptions;
    using Soap.Context.Logging;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public static class MessagePipeline
    {
        public static async Task Execute(ApiMessage originalMessage, MessageMeta meta, BoostrappedContext bootstrappedContext)
        {
            {
                var message = await FillMessageFromStorageIfApplicable(originalMessage); //* replaces with message from blob

                ContextWithMessageLogEntry matureContext = null;

                await PrepareContext(bootstrappedContext, meta, originalMessage, message, v => matureContext = v);

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

            static async Task PrepareContext(
                BoostrappedContext boostrappedContext,
                MessageMeta meta,
                ApiMessage messageAtPerimeter,
                ApiMessage message,
                Action<ContextWithMessageLogEntry> setContext)
            {
                try
                {
                    
                    var contextWithMessageLogEntry = await boostrappedContext.Upgrade(message, messageAtPerimeter, meta);

                    setContext(contextWithMessageLogEntry);
                }
                catch (Exception e)
                {
                    //* don't use guards in the pipeline code, it will mask the underlying error
                    throw new CircuitException("Cannot complete context preparation", e);
                }
            }

            async Task<ApiMessage> FillMessageFromStorageIfApplicable(ApiMessage originalMessage)
            {
                var blobId = originalMessage.Headers.GetBlobId();
                if (blobId.HasValue)
                {
                    return await bootstrappedContext.BlobStorage.GetApiMessageFromBlob(blobId.Value);
                }
                else
                {
                    return originalMessage;
                }
            }

            async Task ProcessMessage(ContextWithMessageLogEntry context)
            {
                var msg = context.Message;

                var result = await context.AttemptToFinishAnUnfinishedUnitOfWork();

                switch (result)
                {
                    case UnitOfWork.State.AllComplete:
                        
                        //* give this time to churn in the background, but by this time it needs to be done otherwise you shouldn't log success
                        await context.HasBeenUploadedToBlobStorageIfNecessary;
                        
                        context.SerilogSuccess();
                        return;
                    case UnitOfWork.State.AllRolledBack:
                        
                        //* give this time to churn in the background, but by this time it needs to be done otherwise you shouldn't log success
                        await context.HasBeenUploadedToBlobStorageIfNecessary;
                        
                        //* don't try again after a rollback as you may be out of retries to rollback again
                        throw new DomainExceptionWithErrorCode(UnitOfWorkErrorCodes.UnitOfWorkFailedUnitOfWorkRolledBack);

                    case UnitOfWork.State.New:

                        /* validate here rather than beginning of method because there can be validation logic that varies between attempts and you want
                                 to attempt to finish an unfinished uow if possible first */
                        msg.ValidateOrThrow(context);
                        
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
                        
                        //* give this time to churn in the background, but by this time it needs to be done otherwise you shouldn't log success
                        await context.HasBeenUploadedToBlobStorageIfNecessary;
                        
                        context.SerilogSuccess();
                        break;
                }
            }

            async Task HandleFailure(ContextWithMessageLogEntry context, Exception exception)
            {
                Exception exceptionThrownToContext;

                try //- log the message failure
                {
                    var exceptionMessages = new FormattedExceptionInfo(exception, context.AppConfig);

                    await context.TakeFailureActions(exceptionMessages);

                    context.SerilogFailure(exceptionMessages);

                    exceptionThrownToContext = exceptionMessages.ExceptionThrownToContext();
                }
                catch (Exception exceptionHandlingException)
                {
                    try //- log the second phase error with some detail
                    {
                        var originalExceptionPlusHandlingException = new ExceptionHandlingException(
                            new AggregateException(exceptionHandlingException, exception));

                        var exceptionMessages = new FormattedExceptionInfo(originalExceptionPlusHandlingException, context.AppConfig);

                        context.Logger.Fatal("Cannot write error to db message log {@details}", exceptionMessages);

                        exceptionThrownToContext = exceptionMessages.ExceptionThrownToContext();
                    }
                    catch (Exception lastChanceException) //- log a minimal error message of last resort
                    {
                        /* avoid use of PipelineExceptionMessages here as it theoretically could be the source of the error
                        * this goes raw to the caller so show don't show any exception details
                        * Serilog should swallow it's own internal errors so logging attempts that way should be safe here
                        */

                        context.Logger.Fatal(
                            "Could not log error to db message log or seq using standard code, ignoring previous error and logging only the raw exception"
                            + "{@messageId} {@originalException} {@secondException} {@finalException}",
                            exception,
                            exceptionHandlingException,
                            lastChanceException);

                        var lastChanceExceptionMessageForCaller =
                            $"Type:{FormattedExceptionInfo.ErrorSourceType.EXWHEX}---Code:000---Could not log error to db message log or seq using standard code.";

                        exceptionThrownToContext = new Exception(lastChanceExceptionMessageForCaller);
                    }
                }

                throw exceptionThrownToContext;
            }
        }
    }
}
