namespace Soap.Interfaces
{
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IStatefulProcess : IBeginStatefulProcess,
                                        IContinueStatefulProcess,
                                        ICanCall<IOperation>,
                                        ICanCall<IProcess>,
                                        ICanCall<IQuery>
    {
    }

    public interface IBeginStatefulProcess
    {
        Task BeginProcess<TMessage>(TMessage message) where TMessage : ApiCommand;
    }

    public interface IContinueStatefulProcess
    {
        Task ContinueProcess<TMessage>(TMessage message) where TMessage : ApiMessage;
    }
}