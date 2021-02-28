namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.NotificationServer.Channels;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;

    public class P203_MessageFailedAllRetries__NotifyOfFinalFailure : Process, IBeginProcess<MessageFailedAllRetries>
    {
        public Func<MessageFailedAllRetries, Task> BeginProcess =>
            async message =>
                {
                var failedMessage = message.ToApiMessage();

                /* Meta.UserProfileOrNull will be null because this message is always sent
                 under the service level authority. So we need to define the email addresses from
                 somewhere else. */

                await NotificationServer.Notify(new Notification(
                        $"The message with id {failedMessage.Headers.GetMessageId()} has failed the maximum number of times.",
                        @$"The failed content was {failedMessage.ToJson(SerialiserIds.ApiBusMessage, true)}."),
                    false,
                    new EmailNotificationSpecificNotificationMeta(
                        "ft.engineering@roseandmy.work",
                        "ft.engineering@roseandmy.work")
                    );
                };
    }
}
