namespace Soap.If.Interfaces
{
    using Soap.If.Interfaces.Messages;

    public interface ISendCommandOperation : IBusOperation
    {
        IApiCommand Command { get; set; }
    }
}