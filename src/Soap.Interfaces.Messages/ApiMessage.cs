﻿namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        public MessageHeaders()
        {

        }
    }

    public static class MessageHeaderExtensions
    {

        public static void EnsureRequiredHeaders(this MessageHeaders headers)
        {
            /* needs to be called everywhere you can send a message (e.g. tests, bus)
             using constructors will just be avoided and since you have to have
            public paramaterless ctor for serialsation no point, and 
            properties just break serialisation*/

            headers.SetTimeOfCreationAtOrigin();
            if (headers.GetMessageId() == Guid.Empty) headers.SetMessageId(Guid.NewGuid());
        }

        public static string GetIdentityToken(this MessageHeaders m)
        {
            m.TryGetValue(Keys.IdentityToken, out string x);
            return x;
        }

        public static Guid GetMessageId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.MessageId, out string x);
            Guid.TryParse(x, out Guid result);
            return result;
        }

        public static StatefulProcessId? GetStatefulProcessId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.StatefulProcessId, out string x);
            if (x is null) return null;

            var values = x.Split(':');
            return new StatefulProcessId
            {
                InstanceId = Guid.Parse(values[1]),
                TypeId = values[0]
            };
        }

        public static void SetTimeOfCreationAtOrigin(this MessageHeaders m)
        {
            m[MessageHeaderExtensions.Keys.TimeOfCreationAtOrigin] = DateTime.UtcNow.ToString("s");
        }
        public static DateTime? GetTimeOfCreationAtOrigin(this MessageHeaders m)
        {
            
            var x = m[Keys.TimeOfCreationAtOrigin];
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

        public static void SetStatefulProcessId(this MessageHeaders m, StatefulProcessId id) =>
            m[Keys.StatefulProcessId] = id.ToString();

        public class Keys
        {
            internal const string IdentityToken = nameof(IdentityToken);

            public const string MessageId = nameof(MessageId);

            internal const string StatefulProcessId = nameof(StatefulProcessId);

            public const string TimeOfCreationAtOrigin = nameof(TimeOfCreationAtOrigin);
        }
    }
}