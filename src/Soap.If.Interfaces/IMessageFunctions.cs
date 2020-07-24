namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Blended;

    public interface IMessageFunctionsServerSide
    {
        Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings();

        Task Handle(ApiMessage msg);

        Task HandleFinalFailure(MessageFailedAllRetries msg);

        void Validate(ApiMessage msg);
    }

    public interface IMessageFunctionsClientSide<T> : ICanCall<IOperation>, ICanCall<IProcess>, ICanCall<IQuery>, ICanCall<IStatefulProcess>
        where T : ApiMessage
    {
        Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        IContinueProcess<T>[]  HandleWithTheseStatefulProcesses { get; } 

        Task Handle(T msg);

        Task HandleFinalFailure(MessageFailedAllRetries<T> msg);

        void Validate(T msg);
    }

}