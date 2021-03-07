namespace Soap.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Soap.Interfaces.Messages;

    /// <summary>
    ///     a way to enforce the signature for handling a msg
    /// </summary>
    /// can only be started with a command but can be continued by anything
    public interface IBeginProcess<in TStarter> where TStarter : IStartAProcess
    {
        Func<TStarter, Task> BeginProcess { get; }
    }
}
