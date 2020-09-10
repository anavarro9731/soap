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
        public MessageHeaders Headers { get; } = new MessageHeaders();

        public abstract ApiPermission Permission { get; }
    }

    public class MessageHeaders : Dictionary<string, string>
    {
    }

    public static class MessageHeaderExtensions
    {
        public static void EnsureRequiredHeaders(this MessageHeaders headers)
        {
            /* needs to be called everywhere you can send a message (e.g. tests, bus)
             using constructors will just be avoided and since you have to have
            public parameterless ctor for serialisation no point, and 
            properties just break serialisation. */

            if (headers.GetTimeOfCreationAtOrigin() == null) headers.SetTimeOfCreationAtOrigin();
            if (headers.GetMessageId() == Guid.Empty) headers.SetMessageId(Guid.NewGuid());
        }

        public static string GetIdentityToken(this MessageHeaders m)
        {
            m.TryGetValue(Keys.IdentityToken, out var x);
            return x;
        }
        
        public static string GetQueueName(this MessageHeaders m)
        {
            m.TryGetValue(Keys.QueueName, out var x);
            return x;
        }

        public static string GetTopic(this MessageHeaders m)
        {
            m.TryGetValue(Keys.Topic, out var x);
            return x;
        }
        
        public static string GetAzureSequenceNumber(this MessageHeaders m)
        {
            m.TryGetValue(Keys.AzureSequenceNumber, out var x);
            return x;
        }

        
        public static Guid GetMessageId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.MessageId, out var x);
            Guid.TryParse(x, out var result);
            return result;
        }

        public static StatefulProcessId? GetStatefulProcessId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.StatefulProcessId, out var x);
            if (x is null) return null;

            var values = x.Split(':');
            return new StatefulProcessId
            {
                InstanceId = Guid.Parse(values[1]), TypeId = values[0]
            };
        }

        public static DateTime? GetTimeOfCreationAtOrigin(this MessageHeaders m)
        {
            m.TryGetValue(Keys.TimeOfCreationAtOrigin, out var x);
            var y = string.IsNullOrWhiteSpace(x) ? (DateTime?)null : DateTime.Parse(x);
            return y;
        }

        public static bool HasStatefulProcessId(this MessageHeaders m) => m.ContainsKey(Keys.StatefulProcessId);

        public static MessageHeaders SetIdentityToken(this MessageHeaders m, string identityToken)
        {
            m[Keys.IdentityToken] = identityToken;
            return m;
        }

        public static MessageHeaders SetMessageId(this MessageHeaders m, Guid messageId)
        {
            m[Keys.MessageId] = messageId.ToString();
            return m;
        }
        
        public static MessageHeaders SetTopic(this MessageHeaders m, string topic)
        {
            m[Keys.Topic] = topic;
            return m;
        }
        
        public static MessageHeaders SetQueueName(this MessageHeaders m, string queueName)
        {
            m[Keys.QueueName] = queueName;
            return m;
        }
        
        public static MessageHeaders SetAzureSequenceNumber(this MessageHeaders m, string queueName)
        {
            m[Keys.QueueName] = queueName;
            return m;
        }

        public static void SetStatefulProcessId(this MessageHeaders m, StatefulProcessId id) =>
            m[Keys.StatefulProcessId] = id.ToString();

        public static void SetTimeOfCreationAtOrigin(this MessageHeaders m)
        {
            var s = DateTime.UtcNow.ToString("s");
            m[Keys.TimeOfCreationAtOrigin] = s;
        }

        public class Keys
        {
            public const string MessageId = nameof(MessageId);

            public const string TimeOfCreationAtOrigin = nameof(TimeOfCreationAtOrigin);

            internal const string IdentityToken = nameof(IdentityToken);

            internal const string StatefulProcessId = nameof(StatefulProcessId);

            internal const string QueueName = nameof(QueueName);

            internal const string Topic = nameof(Topic);

            internal const string AzureSequenceNumber = nameof(AzureSequenceNumber);
        }
    }
}