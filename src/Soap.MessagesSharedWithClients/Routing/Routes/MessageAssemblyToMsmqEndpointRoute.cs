namespace Soap.Pf.ClientServerMessaging.Routing.Routes
{
    using System.Linq;
    using System.Reflection;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public class MessageAssemblyToMsmqEndpointRoute : MsmqMessageRoute
    {
        public MessageAssemblyToMsmqEndpointRoute(Assembly messagesAssembly, string msmqEndpointAddress)
        {
            MsmqEndpointAddress = new MsmqEndpointAddress(msmqEndpointAddress);

            AssemblyName = messagesAssembly.FullName;

            MessageTypes.AddRange(messagesAssembly.GetTypes().Where(t => t.InheritsOrImplements(typeof(IApiMessage))));
        }

        public string AssemblyName { get; set; }

        protected override string From => AssemblyName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().Assembly.FullName == AssemblyName;
        }
    }
}