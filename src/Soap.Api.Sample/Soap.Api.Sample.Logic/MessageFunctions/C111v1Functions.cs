//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C111v1Functions : IMessageFunctionsClientSide<C111v1_GetRecentTestData>
    {
        public IContinueProcess<C111v1_GetRecentTestData>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C111v1_GetRecentTestData msg) => this.Get<P210_C111__ReturnRecentTestData>().Call(x => x.BeginProcess)(Upgrade(msg));

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C111v1_GetRecentTestData msg)
        {
            msg.Validate();
        }

        /* put the Upgrade function here rather than with the message to stop consumers from being lazy and using it
         to get the latest version rather than providing the most recent values */
        static C111v2_GetRecentTestData Upgrade(C111v1_GetRecentTestData c111v1)
        {
            return new C111v2_GetRecentTestData()
            {
                C111_MaxRecords = c111v1.C111_MaxRecords
            };
        }
    }
}
