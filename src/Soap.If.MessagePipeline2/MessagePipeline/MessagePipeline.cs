namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using DataStore.Models.Messages;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    /// <summary>
    ///     1. Build MetaData to the message
    ///     2. Map it to a specific handler type
    ///     3. Log the overall result of the attempted operation.
    /// </summary>
    public class MessagePipeline
    {
        private readonly IAuthenticateUsers authenticator;

        public MessagePipeline(IAuthenticateUsers authenticator)
        {
            this.authenticator = authenticator;
        }

        public async Task Execute(string messageJson, string assemblyQualifiedName)
        {
            {
                var receivedAtTick = StopwatchOps.GetStopwatchTimestamp();
                var receivedAt = DateTime.UtcNow;
                MessageLogEntry messageLogEntry = null;
                ApiMessage message;

                try
                {
                    message = JsonSerializer.Deserialize(messageJson, Type.GetType(assemblyQualifiedName)).As<ApiMessage>();
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

                    await message.FindMessageLogEntry(v => messageLogEntry = v);

                    var messageHasNotBeenSeenBefore = messageLogEntry == null;
                    
                    if (messageHasNotBeenSeenBefore)
                    {
                        await message.CreateNewLogEntry(v => messageLogEntry = v);
                    }

                    message.Authenticate(this.authenticator, v => identity = v);

                    message.UpdateContextAfterLogEntryObtained(messageLogEntry, (receivedAt, receivedAtTick), identity);

                    await message.ValidateOrThrow();

                    if (await message.HasUnfinishedUnitOfWork())
                    {
                        //basically here the hardest part is check the ds uows 
                        //if there are any uncommittedorrolledback remaining and none superseded try recommitting
                        //otherwise try rollingback thats it! i think
                        //and write some awful unit tests :( :(
                        foreach (var datastoreOperation in MContext.AfterMessageLogEntryObtained.MessageLogEntry.UnitOfWork.DataStoreCreateOperations)
                            if (!await datastoreOperation.IsCommittedSuccessfully())
                            {
                                try
                                {
                                    var aggregate = datastoreOperation.FromSerialisableObject<dynamic>();
                                 //   MContext.DataStore.DocumentRepository.AddAsync(new UpdateOperation<dynamic>() {  aggregate)};
                                }
                                catch (DBConcurrencyException)
                                {
                                    //- rollback the rest of the already committed UOW
                                }
                            }
                        foreach (var datastoreOperation in MContext.AfterMessageLogEntryObtained.MessageLogEntry.UnitOfWork.DataStoreUpdateOperations)
                            if (!await datastoreOperation.IsCommittedSuccessfully())
                            {
                                var aggregate = datastoreOperation.FromSerialisableObject<dynamic>();
                                MContext.DataStore.Update(aggregate);
                            }
                        foreach (var datastoreOperation in MContext.AfterMessageLogEntryObtained.MessageLogEntry.UnitOfWork.DataStoreDeleteOperations)
                            if (!await datastoreOperation.IsCommittedSuccessfully())
                            {
                                var aggregate = datastoreOperation.FromSerialisableObject<dynamic>();
                                MContext.DataStore.DeleteHard(aggregate);
                            }

                        /* race condition is possible from IsComplete(), etag will help to resolve difference between uow save and
                        uow re-attempt in which case we will assume the other    write was newer and abandon ours. */
                        await MContext.DataStore.CommitChanges();

                        foreach (var busMessageUnitOfWork in MContext.AfterMessageLogEntryObtained.MessageLogEntry.UnitOfWork.BusCommandMessages)
                            if (!await busMessageUnitOfWork.IsComplete())
                            {
                                var command = busMessageUnitOfWork.FromSerialisableObject<ApiCommand>();
                                MContext.Bus.Send(command);
                            }

                        foreach (var busMessageUnitOfWork in MContext.AfterMessageLogEntryObtained.MessageLogEntry.UnitOfWork.BusEventMessages)
                            if (!await busMessageUnitOfWork.IsComplete())
                            {
                                var @event = busMessageUnitOfWork.FromSerialisableObject<ApiEvent>();
                                MContext.Bus.Publish(@event);
                            }
                        //- race condition is possible from IsComplete() but messagelog will filter it
                        await MContext.Bus.CommitChanges();



                        message.SerilogSuccess();
                        return;
                    }

                    if (message.IsFailedAllRetriesMessage)
                    {
                        message.HandleFinalFailure();
                    }
                    else
                    {
                        switch (message)
                        {
                            case IApiQuery q:
                                var responseEvent = await q.Handle();
                                MContext.Bus.Publish(responseEvent);
                                break;
                            case ApiCommand c:
                                await c.Handle();
                                break;
                            case ApiEvent e:
                                await e.Handle();
                                break;
                        }
                    }

                    await QueuedStateChanges.SaveUnitOfWork();

                    await QueuedStateChanges.CommitChanges();

                    //- update UOW with etags?

                    message.SerilogSuccess();
                }
                catch (Exception exception)
                {
                    Exception finalException;

                    try //- log the message failure
                    {
                        var exceptionMessages = new FormattedExceptionInfo(exception);

                        finalException = exceptionMessages.ToEnvironmentSpecificError();

                        await message.MarkFailureInMessageLog(exceptionMessages);

                        message.SerilogFailure(exceptionMessages);
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
            public static readonly Guid ForceFailBeforeMessageCompletesAndFailErrorHandlerId = Guid.Parse("c7c607b3-fc47-4dbd-81da-8ef28c785a2f");

            public static readonly Guid ForceFailBeforeMessageCompletesId = Guid.Parse("006717f1-fe50-4d0b-b762-75b883ba4a65");
        }
    }
}