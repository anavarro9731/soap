namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Blended;

    public interface IMessageFunctionsServerSide
    {
        Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings();

        Task Handle(ApiMessage msg);

        Task HandleFinalFailure(MessageFailedAllRetries msg);

        void Validate(ApiMessage msg);
    }

    public interface IMessageFunctionsClientSide<T> : ICanCall<IOperation>, ICanCall<IProcess>, ICanCall<IQuery>
        where T : ApiMessage
    {
        Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        Type[]  MessageCanContinueTheseStatefulProcesses { get; }

        Task Handle(T msg);

        Task HandleFinalFailure(MessageFailedAllRetries<T> msg);

        void Validate(T msg);
    }

    public interface IOperation : ICanCall<IQuery>
    {
    }

    public interface IProcess : ICanCall<IOperation>, ICanCall<IProcess>, ICanCall<IQuery>
    {
    }

    public interface IQuery
    {
    }

    public class CallManager<TProcessOrOperation> where TProcessOrOperation : class, new()
    {
        public Func<TIn, TOut> Exec<TIn, TOut>(Func<TProcessOrOperation, Func<TIn, TOut>> method) =>
            method(new TProcessOrOperation());

        public Func<TOut> Exec<TOut>(Func<TProcessOrOperation, Func<TOut>> method) => method(new TProcessOrOperation());
    }

    public interface ICanCall<in T>
    {
    }

    public static class CanCallExtensions
    {
        public static CallManager<TProcessOrOperation> Get<TProcessOrOperation>(this ICanCall<TProcessOrOperation> caller)
            where TProcessOrOperation : class, new() =>
            new CallManager<TProcessOrOperation>();
    }
}