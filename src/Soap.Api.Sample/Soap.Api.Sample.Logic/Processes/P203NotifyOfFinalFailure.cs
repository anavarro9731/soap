namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;

    public class P203NotifyOfFinalFailure : Process, IBeginProcess<MessageFailedAllRetries>
    {
        public Func<MessageFailedAllRetries, Task> BeginProcess =>
            async message =>
                {
                var failedMessage = message.ToApiMessage();
                
                await NotificationServer.Notify(
                    new Notification
                    {
                        Subject =
                            @$"The message with id {failedMessage.Headers.GetMessageId()} has failed the maximum number of times.
                        The failed content was {failedMessage.ToJson(SerialiserIds.ApiBusMessage, true)}."
                    });
                };
    }
}
