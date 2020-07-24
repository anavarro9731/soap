namespace Soap.Interfaces
{
    public interface IProcess : ICanCall<IOperation>, ICanCall<IProcess>, ICanCall<IQuery>
    {
    }
}