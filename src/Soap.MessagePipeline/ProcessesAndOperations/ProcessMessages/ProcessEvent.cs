namespace Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages
{
    using CircuitBoard.Messages;

    public class ProcessEvent : IMessage
    {
        public ProcessEvent(string processType, string username)
        {
            ProcessType = processType;
            Username = username;
        }

        public string ProcessType { get; set; }

        public string Username { get; set; }
    }
}