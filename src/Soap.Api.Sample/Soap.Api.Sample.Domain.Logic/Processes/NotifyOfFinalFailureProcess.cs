namespace Sample.Logic.Processes
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.NotificationServer;

    public class NotifyOfFinalFailureProcess : Process, IBeginProcess<MessageFailedAllRetries>
    {
        public Func<MessageFailedAllRetries, Task> BeginProcess =>
            async message =>
                {
                await NotificationServer.Notify(
                    new Notification
                    {
                        Subject = @$"The message with id {message.FailedMessage.MessageId} has failed the maximum number of times.
                        The failed content was {JsonSerializer.Serialize(message.FailedMessage)}."
                    });
                };
    }
}