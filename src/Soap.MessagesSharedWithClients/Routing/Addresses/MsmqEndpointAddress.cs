namespace Soap.Pf.ClientServerMessaging.Routing.Addresses
{
    using System.Text.RegularExpressions;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class MsmqEndpointAddress
    {
        public MsmqEndpointAddress(string msmqEndpointAddress)
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