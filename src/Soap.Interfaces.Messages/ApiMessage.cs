namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Messages;

    /*
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variable logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 

    Interfaces are nice and can now have default impl but they can't access the impl inside the class
    one big downside, and they clutter up the class with all the repetitive properties that don't need
    a default impl so in most cases abstract classes are still preferable where a Is-A relation exists
     */

    public abstract class ApiMessage : IMessage
    {
        public MessageHeaders Headers { get; set; } = new MessageHeaders();

        public string IdentityToken { get; set; }

        public Guid MessageId { get; set; }

        public abstract ApiPermission Permission { get; }

        public DateTime? TimeOfCreationAtOrigin { get; set; }
    }

    public class MessageHeaders : Dictionary<string, string> {}

    public struct StatefulProcessId
    {
        public StatefulProcessId(string typeId, Guid instanceId)
        {
            TypeId = typeId;
            InstanceId = instanceId;
        }

        public string TypeId { get; set; }

        public Guid InstanceId { get; set; }

        public override string ToString() => $"{TypeId}:{InstanceId}";
    }

    public static class HeaderExtensions
    {
        public class Keys
        {
            internal const string StatefulProcessId = nameof(StatefulProcessId);
        }

        public static bool HasStatefulProcessId(this MessageHeaders m) => m.ContainsKey(nameof(GetStatefulProcessId));

        public static StatefulProcessId GetStatefulProcessId(this MessageHeaders m)
        {
            var values = m[nameof(GetStatefulProcessId)].Split(':');
            return new StatefulProcessId
            {
                InstanceId = Guid.Parse(values[1]), TypeId = values[0]
            };
        }

        public static void SetStatefulProcessId(this MessageHeaders m, StatefulProcessId id) => m[Keys.StatefulProcessId] = id.ToString();
    }
}