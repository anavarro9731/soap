namespace Soap.If.Interfaces.Messages
{
    using System.Threading.Tasks;

    //- marker interface, since ApiCommand<T> is generic making x is y comparisons to identify it as a query difficult
    public interface IApiQuery
    {
        Task<ApiEvent> Handle();
    }
}