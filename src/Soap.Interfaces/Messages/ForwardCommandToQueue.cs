namespace Soap.Interfaces.Messages
{
    public class ForwardCommandToQueue<T> : ApiCommand where T : IApiCommand
    {
        public ForwardCommandToQueue(IApiCommand command)
        {
            Command = command;
        }

        public IApiCommand Command { get; set; }
    }
}