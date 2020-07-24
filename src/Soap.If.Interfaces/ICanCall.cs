namespace Soap.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public interface ICanCall<in T>
    {
    }

    public class CallManager<TCallee> where TCallee : class, new()
    {
        public Func<TIn, TOut> Call<TIn, TOut>(Func<TCallee, Func<TIn, TOut>> method) => method(new TCallee());

        public Func<TOut> Call<TOut>(Func<TCallee, Func<TOut>> method) => method(new TCallee());
    }

    public class StatefulProcessStartManager<TStatefulProcess> where TStatefulProcess : class, IStatefulProcess, new()
    {
        public Func<TIn, Task> Call<TIn>(Func<TStatefulProcess, Func<TIn, Task>> method) where TIn : ApiCommand =>
            new TStatefulProcess().BeginProcess;
    }

    public static class CanCallExtensions
    {
        public static CallManager<TProcessOrOperation> Get<TProcessOrOperation>(this ICanCall<TProcessOrOperation> caller)
            where TProcessOrOperation : class, new() =>
            new CallManager<TProcessOrOperation>();

        public static StatefulProcessStartManager<TStatefulProcess> Get<TStatefulProcess>(this ICanCall<IStatefulProcess> caller)
            where TStatefulProcess : class, IStatefulProcess, new() =>
            new StatefulProcessStartManager<TStatefulProcess>();
    }
}