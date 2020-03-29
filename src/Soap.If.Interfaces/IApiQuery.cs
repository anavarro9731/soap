namespace Soap.Interfaces.Messages
{
    //* marker interface, since ApiCommand<T> is generic making x is y comparisons to identify it as a query difficult
    public interface IApiQuery : IApiCommand
    {
    }
}