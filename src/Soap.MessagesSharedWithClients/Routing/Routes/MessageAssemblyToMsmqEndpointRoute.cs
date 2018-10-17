namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System.Reflection;
    using Soap.If.Interfaces.Messages;
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public class MessageAssemblyToMsmqEndpointRoute : MsmqMessageRoute
    {
        public MessageAssemblyToMsmqEndpointRoute(Assembly messagesAssembly, string msmqEndpointAddress)
        {
            MsmqEndpointAddress = new MsmqEndpointAddress(msmqEndpointAddress);

            AssemblyName = messagesAssembly.FullName;
        }

        public string AssemblyName { get; set; }

        protected override string From => AssemblyName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().Assembly.FullName == AssemblyName;
        }
    }
}