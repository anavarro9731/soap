namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P561ReturnForm : Process, IBeginProcess<C109v1_GetForm>
    {
        public Func<C109v1_GetForm, Task> BeginProcess =>
            async message =>
                {
                var eventName =
                    $"{typeof(E500v1_GetC107Form).Namespace}.{message.C109_FormDataEventName}, {typeof(E500v1_GetC107Form).Assembly.GetName().Name}";
                var formDataEventType = Type.GetType(eventName);
                Guard.Against(formDataEventType == null, "Cannot find command of type: " + message.C109_FormDataEventName);
                var @event = Activator.CreateInstance(formDataEventType) as ApiEvent;
                await Publish(@event);
                };
    }
}
