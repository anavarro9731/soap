
namespace Soap.Api.Sample.Messages.Events
{
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class E101v1_GotServiceState : ApiEvent
    {
        public ServiceState E101_State { get; set; }

        public override void Validate()
        {
        }

        public class ServiceState
        {
            public EnumerationFlags E101_DatabaseState { get; set; }
        }
    }
}
