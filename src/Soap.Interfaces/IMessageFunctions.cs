namespace Soap.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    public interface IMessageFunctionsServerSide
    {
        Task Handle(ApiMessage msg);

        Task HandleFinalFailure(MessageFailedAllRetries msg);

        void Validate(ApiMessage msg);
    }

    public interface IMessageFunctionsClientSide<T> : ICanCall<IOperation>,
                                                      ICanCall<IProcess>,
                                                      ICanCall<IQuery>,
                                                      ICanCall<IStatefulProcess> where T : ApiMessage
    {
        
        IContinueProcess<T>[] HandleWithTheseStatefulProcesses { get; }

        Task Handle(T msg);

        Task HandleFinalFailure(MessageFailedAllRetries msg);

        void Validate(T msg);
    }
}