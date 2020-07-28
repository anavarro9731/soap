namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.NotificationServer;
    using Soap.Utility.Functions.Extensions;

    public class P557NotifyOfFinalFailure : Process, IBeginProcess<MessageFailedAllRetries>
    {
        public Func<MessageFailedAllRetries, Task> BeginProcess =>
            async message =>
                {
                await NotificationServer.Notify(
                    new Notification
                    {
                        Subject =
                            @$"The message with id {message.FailedMessage.Headers.GetMessageId()} has failed the maximum number of times.
                        The failed content was {message.FailedMessage.ToNewtonsoftJson()}."
                    });
                };
    }
}