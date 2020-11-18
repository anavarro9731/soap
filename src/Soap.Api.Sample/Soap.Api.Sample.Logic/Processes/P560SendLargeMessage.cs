namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P560SendLargeMessage : Process, IBeginProcess<C105v1SendLargeMessage>
    {
        public Func<C105v1SendLargeMessage, Task> BeginProcess => async message => { await Bus.Send(new C106v1LargeCommand()); };
    }
}