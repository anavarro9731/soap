namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public abstract ApiPermission Permission { get; }
    }
    
    public class MessageHeaders : List<Enumeration>
    {
        /* the purpose for this class (modelling headers as a dictionary) is really to allow for backwards compatability
         as the system grows, however, it also causes an issue with dictionary keys changing case and then not matching again 
         since they are modeled as objects. which is why we have the naming strategy set to ignore dictionary keys on
         message serialisation. this could also be checked in the header extensions methods (e.g. toLower()) if need be */
    }

    public static class MessageHeaderExtensionsA
    {
        public static void SetAndCheckHeadersOnOutgoingCommand(this MessageHeaders messageHeaders, ApiMessage message)
        {
            messageHeaders.SetTimeOfCreationAtOrigin();
            messageHeaders.SetMessageId(Guid.NewGuid()); 
            
            if (message is MessageFailedAllRetries m)
            {
                messageHeaders.SetQueueName(Type.GetType(m.TypeName).Assembly.GetName().Name); //* send to owners queue
            }
            else
            {
                messageHeaders.SetQueueName(message.GetType().Assembly.GetName().Name); //* send to owners queue
            }
            
            messageHeaders.SetSchema(message.GetType().FullName);
            
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All outgoing Api messages must have a valid {nameof(Keys.MessageId)} header");
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All outgoing Api messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");

            Ensure(messageHeaders.GetQueue() != null, $"All outgoing Api commands must have a {Keys.QueueName} header set");
            
            Ensure(messageHeaders.GetSchema() != null, $"All outgoing Api commands must have a {nameof(Keys.Schema)} header set");
            
            /* NOT SET
            identity token optionally present, some message are anonymous though most aren't    
            stateful process id optionally present when stateful process is involved   
            topic n/a
            azure sequence number, allows a scheduled message to be a cancelled later, but not sure its useful on the message itself so not setting yet
            blob id only present on large messages
            BROWSERCLIENT IDS only applicable on outgoing events or incoming commands 
            */
        }

        public static void SetAndCheckHeadersOnOutgoingEvent(this MessageHeaders messageHeaders, ApiMessage message)
        {
            messageHeaders.SetTimeOfCreationAtOrigin();
            messageHeaders.SetMessageId(Guid.NewGuid()); 
            messageHeaders.SetTopic(message.GetType().FullName);
            messageHeaders.SetSchema(message.GetType().FullName);
            
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All outgoing Api messages must have a valid {nameof(Keys.MessageId)} header");
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All outgoing Api messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");

            Ensure(messageHeaders.GetTopic() != null, $"All outgoing Api events must have a {Keys.Topic} header set");

            CheckBrowserClientHeaders(messageHeaders);
            
            Ensure(messageHeaders.GetSchema() != null, $"All outgoing Api events must have a {nameof(Keys.Schema)} header set");
            
            /* NOT SET
            identity token optionally present, some message are anonymous though most aren't    
            stateful process id optionally present when stateful process is involved    
            queue name n/a
            blob id only present on large messages
            */

        }

        public static void SetDefaultHeadersForIncomingTestMessages(this MessageHeaders messageHeaders, ApiMessage message)
        {

            if (messageHeaders.GetMessageId() == Guid.Empty)
            messageHeaders.SetMessageId(Guid.NewGuid());
            
            if (messageHeaders.GetTimeOfCreationAtOrigin() == null)
            messageHeaders.SetTimeOfCreationAtOrigin();
            
            if (string.IsNullOrEmpty(messageHeaders.GetIdentityToken()))
            messageHeaders.SetIdentityToken("identity token");
            
            if (string.IsNullOrEmpty(messageHeaders.GetQueue()))
            messageHeaders.SetQueueName("queue name");
            
            if (string.IsNullOrEmpty(messageHeaders.GetTopic()))
            messageHeaders.SetTopic("topic");
            
            if (messageHeaders.GetCommandConversationId() == null)
            messageHeaders.SetCommandConversationId(Guid.NewGuid());

            if (string.IsNullOrEmpty(messageHeaders.GetCommandHash()))
            messageHeaders.SetCommandHash("command hash");
            
            if (string.IsNullOrEmpty(messageHeaders.GetSchema()))
            messageHeaders.SetSchema(message.GetType().FullName);

            /* NOT SET
             BLOBID
             STATEFULPROCESSID
             BROWSERCLIENT IDS
             */
            
        }
        
        public static void ValidateIncomingMessageHeaders(this MessageHeaders messageHeaders)
        {
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All incoming Api messages must have a valid {nameof(Keys.MessageId)} header");
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All incoming messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");

            CheckBrowserClientHeaders(messageHeaders); 
            Ensure(messageHeaders.GetSchema() != null, $"All incoming Api messages must have a {nameof(Keys.Schema)} header set");
            
            //* not validated
            //* identity token optionally present, some message are anonymous though most aren't    
            //* stateful process id optionally present when stateful process is involved    
            //* queue name not relevant anymore
            //* topic not relevant anymore
            //* azure sequence number not relevant anymore
            //* blob id optional/present on large messages
            
        }
        
        private static void CheckBrowserClientHeaders(MessageHeaders messageHeaders)
        {
            //*command hash optionally present on commands coming from or events going to the client
            if (messageHeaders.GetSessionId() != null)
            {
                Ensure(
                    messageHeaders.GetCommandHash() != null,
                    $"All Api messages with {Keys.SessionId} header set must also have {Keys.CommandHash} set");
                
                Ensure(
                    messageHeaders.GetCommandConversationId() != null,
                    $"All Api messages with {Keys.SessionId} header set must also have {Keys.CommandConversationId} set");
            }
        }

        private static void Ensure(bool acceptable, string errorMessage)
        {
            if (!acceptable) throw new Exception(errorMessage);
        }
    }

    public static class MessageHeaderExtensionsB
    {

        public static Guid? GetBlobId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.BlobId, out var x);
            Guid.TryParse(x, out var result);
            return result == Guid.Empty ? (Guid?)null : result;
        }
        
        public static string GetCommandHash(this MessageHeaders m)
        {
            m.TryGetValue(Keys.CommandHash, out var x);
            return x;
        }

        public static Guid? GetCommandConversationId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.CommandConversationId, out var x);
            Guid.TryParse(x, out var result);
            return result == Guid.Empty ? (Guid?)null : result;
        }

        public static string GetIdentityToken(this MessageHeaders m)
        {
            m.TryGetValue(Keys.IdentityToken, out var x);
            return x;
        }

        public static Guid GetMessageId(this MessageHeaders m)
        {
            //* make an exception and return Guid and not Guid? since should always be set, and avoid .Value
            // check for Guid.Empty in validation
            m.TryGetValue(Keys.MessageId, out var x);
            Guid.TryParse(x, out var result);
            return result;
        }

        public static string GetQueue(this MessageHeaders m)
        {
            m.TryGetValue(Keys.QueueName, out var x);
            return x;
        }
        
        public static string GetSessionId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.SessionId, out var x);
            return x;
        }

        public static string GetSchema(this MessageHeaders m)
        {
            m.TryGetValue(Keys.Schema, out var x);
            return x;
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

        public static string GetTopic(this MessageHeaders m)
        {
            m.TryGetValue(Keys.Topic, out var x);
            return x;
        }

        private static void TryGetValue(this List<Enumeration> lessErrorProneSerialisableDictionary, string key, out string value)
        {
            var v = lessErrorProneSerialisableDictionary.SingleOrDefault(x => x.Key == key);
            value = v?.Value;
        }

        public static bool HasStatefulProcessId(this MessageHeaders m) => m.Exists(v => v.Key == Keys.StatefulProcessId);

        public static MessageHeaders SetBlobId(this MessageHeaders m, Guid blobId)
        {
            if (!m.Exists(v => v.Key ==Keys.BlobId))
            m.Add(new Enumeration(Keys.BlobId, blobId.ToString()));

            return m;
        }
        
        public static MessageHeaders SetCommandHash(this MessageHeaders m, string commandHash)
        {
            if (!m.Exists(v => v.Key ==Keys.CommandHash))
            m.Add(new Enumeration(Keys.CommandHash, commandHash));
            return m;
        }

        public static MessageHeaders SetCommandConversationId(this MessageHeaders m, Guid conversationId)
        {
            if (!m.Exists(v => v.Key ==Keys.CommandConversationId))
            m.Add(new Enumeration(Keys.CommandConversationId, conversationId.ToString()));
            return m;
        }

        public static MessageHeaders SetIdentityToken(this MessageHeaders m, string identityToken)
        {
            if (!m.Exists(v => v.Key ==Keys.IdentityToken))
            m.Add(new Enumeration(Keys.IdentityToken, identityToken));
            return m;
        }

        public static MessageHeaders SetMessageId(this MessageHeaders m, Guid messageId)
        {
            if (!m.Exists(v => v.Key ==Keys.MessageId))
            m.Add(new Enumeration(Keys.MessageId, messageId.ToString()));
            return m;
        }

        public static MessageHeaders SetQueueName(this MessageHeaders m, string queueName)
        {
            if (!m.Exists(v => v.Key ==Keys.QueueName))
            m.Add(new Enumeration(Keys.QueueName, queueName));
            return m;
        }

        public static MessageHeaders SetSchema(this MessageHeaders m, string schema)
        {
            if (!m.Exists(v => v.Key ==Keys.Schema))
            m.Add(new Enumeration(Keys.Schema, schema));
            return m;
        }

        public static MessageHeaders SetStatefulProcessId(this MessageHeaders m, StatefulProcessId id) {
            if (!m.Exists(v => v.Key ==Keys.StatefulProcessId))
            m.Add(new Enumeration(Keys.StatefulProcessId, id.ToString()));
            return m;
        }
        
        public static MessageHeaders SetTimeOfCreationAtOrigin(this MessageHeaders m)
        {
            
            var s = DateTime.UtcNow.ToString("s");
            if (!m.Exists(v => v.Key ==Keys.TimeOfCreationAtOrigin))
            m.Add(new Enumeration(Keys.TimeOfCreationAtOrigin, s));
            return m;
        }

        public static MessageHeaders SetTopic(this MessageHeaders m, string topic)
        {
            if (!m.Exists(v => v.Key == Keys.Topic))
            m.Add(new Enumeration(Keys.Topic, topic));
            return m;
        }
        
        public static MessageHeaders SetSessionId(this MessageHeaders m, string sessionId)
        {
            if (!m.Exists(v => v.Key == Keys.SessionId))
                m.Add(new Enumeration(Keys.SessionId, sessionId));
            return m;
        }

    }
    public class Keys
    {
        //* uniqueId of message
        public const string MessageId = nameof(MessageId);

        //* time when message was created / sent
        public const string TimeOfCreationAtOrigin = nameof(TimeOfCreationAtOrigin);

        //* id of blob if message is large
        internal const string BlobId = nameof(BlobId);

        //* hash of message that started a client side conversation to link it up again (conversationid too specific for cache)
        internal const string CommandHash = nameof(CommandHash);

        //* id of client side conversation
        internal const string CommandConversationId = nameof(CommandConversationId);

        //* auth token
        internal const string IdentityToken = nameof(IdentityToken);

        //* dest queue when its a command
        internal const string QueueName = nameof(QueueName);

        //* short type name
        internal const string Schema = nameof(Schema);

        //* server side conversation id
        internal const string StatefulProcessId = nameof(StatefulProcessId);

        //* dest topic when its an event
        internal const string Topic = nameof(Topic);

        //* servicebus messagesession Id
        internal const string SessionId = nameof(SessionId);
    }
}