namespace Soap.Api.Sample.Messages.Events
{
    using Soap.Interfaces.Messages;

    public class E151v1GotServiceState : ApiEvent
    {
        public override ApiPermission Permission { get; }

        public e151_ServiceState State { get; set; }

        public class e151_ServiceState
        {
            public EnumerationFlags DatabaseState { get; set; }
        }
    }
}