namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;

    public class P206_C105__SendLargeMessage : Process, IBeginProcess<C105v1_SendLargeMessage>
    {
        public Func<C105v1_SendLargeMessage, Task> BeginProcess
        {
            get
            {
                return async message =>
                    {
                        await Bus.Send(new C106v1_LargeCommand().Op(x =>
                            {
                            var messageId = message.C105_C106Id ?? Guid.NewGuid(); 
                            
                            x.Headers.SetMessageId(messageId);
                            }));
                    };
            }
        }
    }
}
