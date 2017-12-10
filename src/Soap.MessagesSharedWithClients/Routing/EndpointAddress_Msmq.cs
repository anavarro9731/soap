namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System.Text.RegularExpressions;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class EndpointAddress_Msmq
    {
        public EndpointAddress_Msmq(string msmqEndpointAddress)
        {
            Guard.Against(
                !Regex.IsMatch(msmqEndpointAddress, "^\\w+@\\w+$"),
                $"Endpoint Address {msmqEndpointAddress} is not well-formed (e.g. queueName@machineName)");

            QueueName = msmqEndpointAddress.SubstringBefore('@');
            MachineName = msmqEndpointAddress.SubstringAfter('@');
        }

        public string MachineName { get; set; }

        public string QueueName { get; set; }

        public override string ToString()
        {
            return $"{QueueName}@{MachineName}";
        }
    }
}