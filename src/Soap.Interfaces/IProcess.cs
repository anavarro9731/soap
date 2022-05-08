namespace Soap.Interfaces
{
    using System.Collections.Generic;

    public interface IProcess : ICanCall<IOperation>, ICanCall<IProcess>, ICanCall<IQuery>
    {
        
    }
}