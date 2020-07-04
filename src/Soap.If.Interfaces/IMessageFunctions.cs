namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Soap.Utility.Objects.Blended;

    public interface IMessageFunctions
    {
        Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings();

        Task HandleFinalFailure(MessageFailedAllRetries msg);

        Task Handle(ApiMessage msg);

        void Validate(ApiMessage msg);
    }

    public interface IMessageFunctions<T> where T: ApiMessage
    {
        Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper();

        Task HandleFinalFailure(MessageFailedAllRetries<T> msg);

        Task Handle(T msg);

        void Validate(T msg);
    }

    
    public interface IOperation
    {
    }
    
    public interface IProcess
    {
    }

    public class CallManager<TProcessOrOperation> where TProcessOrOperation : class, new()
    {
        public Func<TIn, TOut> Exec<TIn, TOut>(Func<TProcessOrOperation, Func<TIn, TOut>> method)
        {
            return method(new TProcessOrOperation());
        }

        
    }

    public interface ICanCall<T, T2> : ICanCall<T>
    {
    }

    public interface ICanCall<in T>
    {
        //CallManager<T2> Use1<T2>() where T2 : class, T, new()
        //{
        //    return new CallManager<T2>();
        //}
    }

    public static class CanCallExtensions
    {
        public static CallManager<TProcessOrOperation> Get<TProcessOrOperation>(this ICanCall<TProcessOrOperation> caller) where TProcessOrOperation : class, new()
        {
            return new CallManager<TProcessOrOperation>();
        }

        //public static CallManager<T> Use2<T>(this ICanCall<T> caller) where T : class, new()
        //{
        //    return (caller).Use1<T>();
        //}
    }

}