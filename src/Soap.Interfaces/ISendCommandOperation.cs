namespace Soap.Interfaces
{
    using Soap.Interfaces.Messages;

    public interface ISendCommandOperation : IBusOperation
    {
        IApiCommand Command { get; set; }
    }
}