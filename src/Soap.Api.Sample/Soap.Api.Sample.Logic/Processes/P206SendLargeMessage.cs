namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P206SendLargeMessage : Process, IBeginProcess<C105v1_SendLargeMessage>
    {
        public Func<C105v1_SendLargeMessage, Task> BeginProcess
        {
            get
            {
                return async message =>
                    {
                    await Bus.Send(new C106v1_LargeCommand());
                    };
            }
        }
    }
}
