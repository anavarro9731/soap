namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Threading.Tasks;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.Integrations.MailGun;
    using Soap.Pf.DomainLogicBase;
    using Soap.Pf.MsmqEndpointBase;

    public class NotifyUsersHandler : CommandHandler<NotifyUsers>
    {
        private readonly INotifyUsers notificationServer;

        public NotifyUsersHandler(ApplicationConfiguration applicationConfiguration, INotifyUsers notificationServer)
        {
            this.notificationServer = notificationServer;
        }

        protected override Task Handle(NotifyUsers message, ApiMessageMeta meta)
        {
            this.notificationServer.Notify(message.Text, message.Subject, message.SendTo);

            return Task.CompletedTask;
        }
    }
}