//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P211_C112__RespondToMessageThatDoesntRequireAuthorisation : Process,
                                                                      IBeginProcess<C112v1_MessageThatDoesntRequireAuthorisation>
    {
        public Func<C112v1_MessageThatDoesntRequireAuthorisation, Task> BeginProcess
        {
            get
            {
                return async message =>
                    {
                    if (message.C112_NextAction.HasFlag(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesntRequireAuthorisation))
                    {
                        await Bus.Send(new C112v1_MessageThatDoesntRequireAuthorisation());
                    }
                    else if (message.C112_NextAction.HasFlag(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction
                                 .SendAnotherCommandThatDoesRequireAuthorisation))
                    {
                        await Bus.Send(new C100v1_Ping());
                    }
                    else if (message.C112_NextAction.HasFlag(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.DoNothing))
                    {
                        //* do nothing
                    }
                    };
            }
        }
    }
}
