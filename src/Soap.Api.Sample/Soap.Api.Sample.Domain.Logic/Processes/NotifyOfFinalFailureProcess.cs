namespace Sample.Logic.Processes
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Sample.Logic.Operations;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.NotificationServer;

    public class NotifyOfFinalFailureProcess : Process, IBeginProcess<MessageFailedAllRetries>
    {
        private readonly ServiceStateOperations serviceStateOperations = new ServiceStateOperations();

        public Func<MessageFailedAllRetries, Task> BeginProcess =>
            async message =>
                {
                await this.context.NotificationServer.Notify(
                    new Notification
                    {
                        Subject = @$"The message with id {message.FailedMessage.MessageId} has failed the maximum number of times.
                        The failed content was {JsonSerializer.Serialize(message.FailedMessage)}."
                    });
                };
    }
}