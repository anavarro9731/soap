namespace Soap.If.Interfaces.Messages
{
    public class ForwardCommandToQueue<TApiCommand> : ApiCommand where TApiCommand : IApiCommand
    {
        public ForwardCommandToQueue(TApiCommand command)
        {
            Command = command;
        }

        public IApiCommand Command { get; set; }
    }
}