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
        public MessageHeaders Headers { get; } = new MessageHeaders();

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
            messageHeaders.EnsureRequiredHeaders();

            if (message is MessageFailedAllRetries m)
            {
                messageHeaders.SetQueueName(Type.GetType(m.TypeName).Assembly.GetName().Name); //* send to owners queue
            }
            else
            {
                messageHeaders.SetQueueName(message.GetType().Assembly.GetName().Name); //* send to owners queue
            }
            
            messageHeaders.SetSchema(message.GetType().FullName);

            /* 1 */
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All outgoing Api messages must have a valid {nameof(Keys.MessageId)} header");
            /* 2 */
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All outgoing Api messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");
            /* 3 */ //* identity token optionally present, some message are anonymous though most aren't    
            /* 4 */ //* stateful process id optionally present when stateful process is involved   
            /* 5 */
            Ensure(messageHeaders.GetQueue() != null, $"All outgoing Api commands must have a {Keys.QueueName} header set");
            /* 6 */ /* topic n/a
            /* 7 */
            //* azure sequence number, allows a scheduled message to be a cancelled later, but not sure its useful on the message itself so not setting yet
            /* 8 */ //* blob id only present on large messages
            /* 9 */ //* conversation id n/a only present on outgoing events or incoming commands
            /* 10 */ //* command hash n/a only present on outgoing events or incoming commands
            /* 11 */ //* channel n/a only present on outgoing events or incoming commands
            /* 12 */
            Ensure(messageHeaders.GetSchema() != null, $"All outgoing Api commands must have a {nameof(Keys.Schema)} header set");
        }

        public static void SetAndCheckHeadersOnOutgoingEvent(this MessageHeaders messageHeaders, ApiMessage message)
        {
            messageHeaders.EnsureRequiredHeaders();
            messageHeaders.SetTopic(message.GetType().FullName);
            messageHeaders.SetChannel("events");
            messageHeaders.SetSchema(message.GetType().FullName);

            /* 1 */
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All outgoing Api messages must have a valid {nameof(Keys.MessageId)} header");
            /* 2 */
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All outgoing Api messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");
            /* 3 */ //* identity token optionally present, some message are anonymous though most aren't    
            /* 4 */ //* stateful process id optionally present when stateful process is involved    
            /* 5 */ //* queue name n/a    
            /* 6 */
            Ensure(messageHeaders.GetTopic() != null, $"All outgoing Api events must have a {Keys.Topic} header set");
            /* 7 */ //*azure sequence number n/a in test
            /* 8 */ //*blob id only present on large messages
            /* 9 */
            CheckCommandHash(messageHeaders);
            /* 10 */
            CheckConversationId(messageHeaders);
            /* 11 */
            Ensure(messageHeaders.GetChannel() != null, $"All outgoing Api events must have a {nameof(Keys.Channel)} header set");
            /* 12 */
            Ensure(messageHeaders.GetSchema() != null, $"All outgoing Api events must have a {nameof(Keys.Schema)} header set");
        }

        public static void SetDefaultHeadersForIncomingTestMessages(this MessageHeaders messageHeaders, ApiMessage message)
        {

            /* 1 */
            if (messageHeaders.GetMessageId() == Guid.Empty)
            messageHeaders.SetMessageId(Guid.NewGuid());
            /* 2 */
            if (messageHeaders.GetTimeOfCreationAtOrigin() == null)
            messageHeaders.SetTimeOfCreationAtOrigin();
            /* 3 */
            if (string.IsNullOrEmpty(messageHeaders.GetIdentityToken()))
            messageHeaders.SetIdentityToken("identity token");
            /* 4 */
            //messageHeaders.SetStatefulProcessId(new StatefulProcessId("type id", Guid.NewGuid()));
            /* 5 */
            if (string.IsNullOrEmpty(messageHeaders.GetQueue()))
            messageHeaders.SetQueueName("queue name");
            /* 6 */
            if (string.IsNullOrEmpty(messageHeaders.GetTopic()))
            messageHeaders.SetTopic("topic");
            /* 7 */
            if (string.IsNullOrEmpty(messageHeaders.GetAzureSequenceNumber()))
            messageHeaders.SetAzureSequenceNumber("azure bus sequence number");
            /* 8 */
            //messageHeaders.SetBlobId(Guid.Empty); 
            /* 9 */
            if (messageHeaders.GetConversationId() == null)
            messageHeaders.SetConversationId(Guid.NewGuid());
            /* 10 */
            if (string.IsNullOrEmpty(messageHeaders.GetCommandHash()))
            messageHeaders.SetCommandHash("command hash");
            /* 11 */
            if (string.IsNullOrEmpty(messageHeaders.GetChannel()))
            messageHeaders.SetChannel("channel");
            /* 12 */
            if (string.IsNullOrEmpty(messageHeaders.GetSchema()))
            messageHeaders.SetSchema(message.GetType().FullName);
            
        }

        public static void SetHeadersOnSchemaModelMessage(this MessageHeaders messageHeaders, ApiMessage message)
        {
            /* 1 */
            messageHeaders.SetMessageId(Guid.NewGuid());
            /* 2 */
            messageHeaders.SetTimeOfCreationAtOrigin();
            /* 3 */
            messageHeaders.SetIdentityToken("identity token");
            /* 4 */
            messageHeaders.SetStatefulProcessId(new StatefulProcessId("type id", Guid.NewGuid()));
            /* 5 */
            messageHeaders.SetQueueName("queue name");
            /* 6 */
            messageHeaders.SetTopic("topic");
            /* 7 */
            messageHeaders.SetAzureSequenceNumber("azure bus sequence number");
            /* 8 */
            messageHeaders.SetBlobId(Guid.Empty); 
            /* 9 */
            messageHeaders.SetConversationId(Guid.NewGuid());
            /* 10 */
            messageHeaders.SetCommandHash("command hash");
            /* 11 */
            messageHeaders.SetChannel("channel");
            /* 12 */
            messageHeaders.SetSchema(message.GetType().FullName);
        }

        public static void ValidateIncomingMessageHeaders(this MessageHeaders messageHeaders)
        {
            /* 1 */
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All incoming Api messages must have a valid {nameof(Keys.MessageId)} header");
            /* 2 */
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All incoming messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");
            /* 3 */ //* identity token optionally present, some message are anonymous though most aren't    
            /* 4 */ //* stateful process id optionally present when stateful process is involved    
            /* 5 */ //* queue name not relevant anymore
            /* 6 */ //* topic not relevant anymore
            /* 7 */ //* azure sequence number not relevant anymore
            /* 8 */ //* blob id optional/present on large messages
            /* 9 */
            CheckCommandHash(messageHeaders);
            /* 10 */
            CheckConversationId(messageHeaders);
            /* 11 */
            Ensure(!(messageHeaders.GetQueue() != null && messageHeaders.GetChannel() == null), $"All incoming Api commands must have a {nameof(Keys.Channel)} header set");
            /* 12 */
            Ensure(messageHeaders.GetSchema() != null, $"All incoming Api messages must have a {nameof(Keys.Schema)} header set");
        }
        
        public static void EnsureRequiredHeaders(this MessageHeaders headers)
        {
            /* needs to be called everywhere you can send a message (e.g. tests, bus)
             using constructors will just be avoided and since you have to have
            public parameterless ctor for serialisation no point, and 
            properties just break serialisation. */

            if (headers.GetTimeOfCreationAtOrigin() == null) headers.SetTimeOfCreationAtOrigin();
            if (headers.GetMessageId() == Guid.Empty) headers.SetMessageId(Guid.NewGuid());
        }
        
        private static void CheckCommandHash(MessageHeaders messageHeaders)
        {
            //*command hash optionally present on commands coming from or events going to the client
            if (messageHeaders.GetConversationId() != null)
            {
                Ensure(
                    messageHeaders.GetCommandHash() != null,
                    $"All Api messages with {Keys.ConversationId} header set must also have {Keys.CommandHash} set");
            }
        }

        private static void CheckConversationId(MessageHeaders messageHeaders)
        {
            //*conversation id optionally present on commands coming from or events going to the client
            if (messageHeaders.GetCommandHash() != null)
            {
                Ensure(
                    messageHeaders.GetConversationId() != null,
                    $"All Api messages with {Keys.CommandHash} header set must also have {Keys.ConversationId} set");
            }
        }
        
        private static void Ensure(bool acceptable, string errorMessage)
        {
            if (!acceptable) throw new Exception(errorMessage);
        }
    }

    public static class MessageHeaderExtensionsB
    {
        public static string GetAzureSequenceNumber(this MessageHeaders m)
        {
            m.TryGetValue(Keys.AzureSequenceNumber, out var x);
            return x;
        }

        public static Guid? GetBlobId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.BlobId, out var x);
            Guid.TryParse(x, out var result);
            return result == Guid.Empty ? (Guid?)null : result;
        }

        public static string GetChannel(this MessageHeaders m)
        {
            m.TryGetValue(Keys.Channel, out var x);
            return x;
        }

        public static string GetCommandHash(this MessageHeaders m)
        {
            m.TryGetValue(Keys.CommandHash, out var x);
            return x;
        }

        public static Guid? GetConversationId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.ConversationId, out var x);
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

        public static MessageHeaders SetAzureSequenceNumber(this MessageHeaders m, string azureSequenceNumber)
        {
            if (!m.Exists(v => v.Key ==Keys.AzureSequenceNumber))
            m.Add(new Enumeration(Keys.AzureSequenceNumber, azureSequenceNumber));
            return m;
        }

        public static MessageHeaders SetBlobId(this MessageHeaders m, Guid blobId)
        {
            if (!m.Exists(v => v.Key ==Keys.BlobId))
            m.Add(new Enumeration(Keys.BlobId, blobId.ToString()));

            return m;
        }

        public static MessageHeaders SetChannel(this MessageHeaders m, string channel)
        {
            if (!m.Exists(v => v.Key ==Keys.Channel))
            m.Add(new Enumeration(Keys.Channel, channel));
            return m;
        }

        public static MessageHeaders SetCommandHash(this MessageHeaders m, string commandHash)
        {
            if (!m.Exists(v => v.Key ==Keys.CommandHash))
            m.Add(new Enumeration(Keys.CommandHash, commandHash));
            return m;
        }

        public static MessageHeaders SetConversationId(this MessageHeaders m, Guid conversationId)
        {
            if (!m.Exists(v => v.Key ==Keys.ConversationId))
            m.Add(new Enumeration(Keys.ConversationId, conversationId.ToString()));
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

    }
    public class Keys
    {
        //* uniqueId of message
        public const string MessageId = nameof(MessageId);

        //* time when message was created / sent
        public const string TimeOfCreationAtOrigin = nameof(TimeOfCreationAtOrigin);

        //* azure bus id
        internal const string AzureSequenceNumber = nameof(AzureSequenceNumber);

        //* id of blob if message is large
        internal const string BlobId = nameof(BlobId);

        //* id of client side channel
        internal const string Channel = nameof(Channel);

        //* hash of message that started a client side conversation to link it up again (conversationid too specific for cache)
        internal const string CommandHash = nameof(CommandHash);

        //* id of client side conversation
        internal const string ConversationId = nameof(ConversationId);

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
    }
}