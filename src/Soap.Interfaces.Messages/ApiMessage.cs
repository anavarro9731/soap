namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using CircuitBoard;
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
        /* ALL messages must adhere to the following rules:
         1. only contain public properties with both getters and setters (never computed properties)
         2. always have a public paramaterless constructors 
         3. never have any logic in their paramaterless construtor or any of its base classes
         Failure to follow these rules will produce erratic behaviour in multiple areas of the system including: serialisation, client side schema production.
         */

        public MessageHeaders Headers { get; set; } = new MessageHeaders();

        public abstract void Validate();
    }

    public class MessageHeaders : List<Enumeration>
    {
        /* the purpose for this class (modelling headers as a dictionary) is really to allow for backwards compatibility
         as the system grows, however, it also causes an issue with dictionary keys changing case and then not matching again 
         since they are modeled as objects. which is why we have the naming strategy set to ignore dictionary keys on
         message serialisation. this could also be checked in the header extensions methods (e.g. toLower()) if need be */
        
    }

    public static class MessageHeaderExtensionsA
    {
        public static void CheckHeadersOnOutgoingCommand(
            this MessageHeaders messageHeaders,
            ApiMessage message,
            bool authEnabled,
            bool commandRequiresAuth,
            string envPartitionKey)
        {

            if (message is MessageFailedAllRetries m)
            {
                messageHeaders.SetQueueName(Type.GetType(m.TypeName).GetTypeInfo().Assembly.GetName().Name + "." + envPartitionKey); //* send to owners queue
            }
            else
            {
                messageHeaders.SetQueueName(message.GetType().GetTypeInfo().Assembly.GetName().Name + "." + envPartitionKey); //* send to owners queue
            }

            messageHeaders.SetSchema(message.GetType().FullName);

            if (authEnabled && commandRequiresAuth)
            {
                Ensure(
                    messageHeaders.GetIdentityChain() != null,
                    "All inter-service outgoing commands from a service must have an identity chain header if auth is enabled and the message is not exempted from authorisation");
            }

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
            access token optionally present, some messages are anonymous
            identitychain should always be present on inter-service commands, as there is always at least one identifiable party, the service
            stateful process id optionally present when stateful process is involved   
            topic n/a
            azure sequence number, allows a scheduled message to be a cancelled later, but not sure its useful on the message itself so not setting yet
            blob id only present on large messages
            sasstoragetoken only present on large commands
            SESSION IDS only applicable on outgoing events or incoming commands 
            */
        }

        public static void CheckHeadersOnOutgoingEvent(this MessageHeaders messageHeaders, ApiMessage message)
        {
            
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All outgoing Api messages must have a valid {nameof(Keys.MessageId)} header");
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All outgoing Api messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");

            Ensure(messageHeaders.GetTopic() != null, $"All outgoing Api events must have a {Keys.Topic} header set");
            
            if (messageHeaders.GetSessionId() != null)
            {
                Ensure(
                    messageHeaders.GetCommandHash() != null,
                    $"All Api messages with {Keys.SessionId} header set must also have {Keys.CommandHash} set");

                Ensure(
                    messageHeaders.GetCommandConversationId() != null,
                    $"All Api messages with {Keys.SessionId} header set must also have {Keys.CommandConversationId} set");
            }

            Ensure(messageHeaders.GetSchema() != null, $"All outgoing Api events must have a {nameof(Keys.Schema)} header set");

            /* NOT SET
            identity token optionally present, some message are anonymous though most aren't
            identitychain only present on commands
            access token not present on events            
            stateful process id optionally present when stateful process is involved    
            queue name n/a
            blob id only present on large messages
            sas token only present on large events
            */
        }


        public static void ValidateIncomingMessageHeaders(this ApiMessage msg)
        {
            var messageHeaders = msg.Headers;
            Ensure(
                messageHeaders.GetMessageId() != null && messageHeaders.GetMessageId() != Guid.Empty,
                $"All incoming messages must have a valid {nameof(Keys.MessageId)} header");
            Ensure(
                messageHeaders.GetTimeOfCreationAtOrigin() != null,
                $"All incoming messages must have a {nameof(Keys.TimeOfCreationAtOrigin)} header set");

            if (messageHeaders.GetSessionId() != null || messageHeaders.GetCommandHash() != null ||messageHeaders.GetCommandConversationId() != null) 
            {
                Ensure(msg is ApiCommand, $"All incoming messages with session headers (sessionid, commandhash, commandconversationid) can only be commands");
                
                Ensure(messageHeaders.GetCommandHash() != null && messageHeaders.GetCommandConversationId() != null && messageHeaders.GetSessionId() != null,
                    $"If providing  session headers (sessionid, commandhash, commandconversationid) you must set all 3");
            }

            if (messageHeaders.GetIdentityToken() != null || messageHeaders.GetAccessToken() != null || messageHeaders.GetIdentityChain() != null)
            {
                Ensure(msg is ApiCommand, $"All incoming messages with auth headers (accesstoken, identitytoken, identitychain) can only be commands");
                
                Ensure(
                    messageHeaders.GetAccessToken() != null && messageHeaders.GetIdentityChain() != null && messageHeaders.GetIdentityToken() != null,
                    $"If providing auth headers (accesstoken, identitytoken, identitychain) you must set all 3");
            }
            
            Ensure(messageHeaders.GetSchema() != null, $"All incoming Api messages must have a {nameof(Keys.Schema)} header set");

            //* not validated
            //* identity token optionally present, some message are anonymous though most aren't
            //* access token optionally present, some messages are anonymous
            //* identitychain optionally present, only set on commands with an identity
            //* stateful process id optionally present when stateful process is involved    
            //* queue name not relevant anymore
            //* topic not relevant anymore
            //* azure sequence number not relevant anymore
            //* blob id optional/present on large messages
            //* sasstoragetoken only present on outgoing UIFormEvents or large incoming commands
        }


        private static void Ensure(bool acceptable, string errorMessage)
        {
            if (!acceptable) throw new CircuitException(errorMessage);
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

        public static Guid? GetCommandConversationId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.CommandConversationId, out var x);
            Guid.TryParse(x, out var result);
            return result == Guid.Empty ? (Guid?)null : result;
        }

        public static string GetCommandHash(this MessageHeaders m)
        {
            m.TryGetValue(Keys.CommandHash, out var x);
            return x;
        }

        public static string GetIdentityToken(this MessageHeaders m)
        {
            m.TryGetValue(Keys.IdentityToken, out var x);
            return x;
        }
        
        public static string GetIdentityChain(this MessageHeaders m)
        {
            m.TryGetValue(Keys.IdentityChain, out var x);
            return x;
        }
        
        public static string GetAccessToken(this MessageHeaders m)
        {
            m.TryGetValue(Keys.AccessToken, out var x);
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

        public static string GetSasStorageToken(this MessageHeaders m)
        {
            m.TryGetValue(Keys.SasStorageToken, out var x);
            return x;
        }

        public static string GetSchema(this MessageHeaders m)
        {
            m.TryGetValue(Keys.Schema, out var x);
            return x;
        }

        public static string GetSessionId(this MessageHeaders m)
        {
            m.TryGetValue(Keys.SessionId, out var x);
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

        public static bool HasStatefulProcessId(this MessageHeaders m) => m.Exists(v => v.Key == Keys.StatefulProcessId);

        public static MessageHeaders SetBlobId(this MessageHeaders m, Guid blobId)
        {
            
            if (!m.Exists(v => v.Key == Keys.BlobId))
            {
                m.Add(new Enumeration(Keys.BlobId, blobId.ToString()));
            }
            else throw new CircuitException($"Cannot set header {Keys.BlobId} because it has already been set");
            

            return m;
        }

        public static MessageHeaders SetCommandConversationId(this MessageHeaders m, Guid conversationId)
        {
            if (!m.Exists(v => v.Key == Keys.CommandConversationId))
            {
                m.Add(new Enumeration(Keys.CommandConversationId, conversationId.ToString()));
            }
            else throw new CircuitException($"Cannot set header {Keys.CommandConversationId} because it has already been set");

            return m;
        }

        public static MessageHeaders SetCommandHash(this MessageHeaders m, string commandHash)
        {
            if (!m.Exists(v => v.Key == Keys.CommandHash))
            {
                m.Add(new Enumeration(Keys.CommandHash, commandHash));
            }            
            else throw new CircuitException($"Cannot set header {Keys.CommandHash} because it has already been set");

            return m;
        }

        public static MessageHeaders SetIdentityToken(this MessageHeaders m, string identityToken)
        {
            if (!m.Exists(v => v.Key == Keys.IdentityToken))
            {
                m.Add(new Enumeration(Keys.IdentityToken, identityToken));
            }
            else throw new CircuitException($"Cannot set header {Keys.IdentityToken} because it has already been set");
            
            return m;
        }
        
        public static MessageHeaders SetAccessToken(this MessageHeaders m, string identityToken)
        {
            if (!m.Exists(v => v.Key == Keys.AccessToken))
            {
                m.Add(new Enumeration(Keys.AccessToken, identityToken));
            }
            else throw new CircuitException($"Cannot set header {Keys.AccessToken} because it has already been set");
            
            return m;
        }
        
        public static MessageHeaders SetIdentityChain(this MessageHeaders m, string identityChain)
        {
            if (!m.Exists(v => v.Key == Keys.IdentityChain))
            {
                m.Add(new Enumeration(Keys.IdentityChain, identityChain));
            }
            else throw new CircuitException($"Cannot set header {Keys.IdentityChain} because it has already been set");
            
            return m;
        }

        public static MessageHeaders SetMessageId(this MessageHeaders m, Guid messageId)
        {
            if (!m.Exists(v => v.Key == Keys.MessageId))
            {
                m.Add(new Enumeration(Keys.MessageId, messageId.ToString()));
            }

            return m;
        }

        public static MessageHeaders SetQueueName(this MessageHeaders m, string queueName)
        {
            if (!m.Exists(v => v.Key == Keys.QueueName))
            {
                m.Add(new Enumeration(Keys.QueueName, queueName));
            }
            else throw new CircuitException($"Cannot set header {Keys.QueueName} because it has already been set");
            
            return m;
        }

        public static MessageHeaders SetSasStorageToken(this MessageHeaders m, string sasStorageToken)
        {
            if (!m.Exists(v => v.Key == Keys.SasStorageToken))
            {
                m.Add(new Enumeration(Keys.SasStorageToken, sasStorageToken));
            }
            else throw new CircuitException($"Cannot set header {Keys.SasStorageToken} because it has already been set");
            
            return m;
        }
        
        public static MessageHeaders SetSchema(this MessageHeaders m, string schema)
        {
            if (!m.Exists(v => v.Key == Keys.Schema))
            {
                m.Add(new Enumeration(Keys.Schema, schema));
            }
            else throw new CircuitException($"Cannot set header {Keys.Schema} because it has already been set");
            
            return m;
        }

        public static MessageHeaders SetSessionId(this MessageHeaders m, string sessionId)
        {
            if (!m.Exists(v => v.Key == Keys.SessionId))
            {
                m.Add(new Enumeration(Keys.SessionId, sessionId));
            }
            else throw new CircuitException($"Cannot set header {Keys.SessionId} because it has already been set");
            
            return m;
        }
        
        public static MessageHeaders ClearSessionHeaders(this MessageHeaders m)
        {
                m.RemoveAll(h => h.Key == Keys.SessionId 
                                 || h.Key == Keys.CommandHash
                                 ||h.Key == Keys.CommandConversationId);
                return m;
        }

        public static MessageHeaders SetStatefulProcessId(this MessageHeaders m, StatefulProcessId id)
        {
            if (!m.Exists(v => v.Key == Keys.StatefulProcessId))
            {
                m.Add(new Enumeration(Keys.StatefulProcessId, id.ToString()));
            }
            else throw new CircuitException($"Cannot set header {Keys.StatefulProcessId} because it has already been set");
            
            return m;
        }

        public static MessageHeaders SetTimeOfCreationAtOrigin(this MessageHeaders m)
        {
            var s = DateTime.UtcNow.ToString("s");
            if (!m.Exists(v => v.Key == Keys.TimeOfCreationAtOrigin))
            {
                m.Add(new Enumeration(Keys.TimeOfCreationAtOrigin, s));
            }
            else throw new CircuitException($"Cannot set header {Keys.TimeOfCreationAtOrigin} because it has already been set");
            
            return m;
        }

        public static MessageHeaders SetTopic(this MessageHeaders m, string topic)
        {
            if (!m.Exists(v => v.Key == Keys.Topic))
            {
                m.Add(new Enumeration(Keys.Topic, topic));
            }
            else throw new CircuitException($"Cannot set header {Keys.Topic} because it has already been set");
            
            return m;
        }

        private static void TryGetValue(this List<Enumeration> lessErrorProneSerialisableDictionary, string key, out string value)
        {
            var v = lessErrorProneSerialisableDictionary.SingleOrDefault(x => x.Key == key);
            value = v?.Value;
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
        
        //* SESSIONS
        internal const string CommandConversationId = nameof(CommandConversationId); //* id of client side conversation
        internal const string CommandHash = nameof(CommandHash); //* hash of message that started a client side conversation to link it up again (conversationId too specific for cache)
        internal const string SessionId = nameof(SessionId); //* sessionid for web socket client 
        
        //* AUTH
        internal const string IdentityToken = nameof(IdentityToken); //* openid identity token
        internal const string AccessToken = nameof(AccessToken); //* oauth bearer token
        internal const string IdentityChain = nameof(IdentityChain); //* comma separated list of identities that have handled messages in this workflow
        
        //* dest queue when its a command
        internal const string QueueName = nameof(QueueName);

        //* token used for uploading and downloading event message blobs 
        internal const string SasStorageToken = nameof(SasStorageToken);
        
        //* short type name
        internal const string Schema = nameof(Schema);

        //* server side conversation id 
        public const string StatefulProcessId = nameof(StatefulProcessId);

        //* dest topic when its an event
        internal const string Topic = nameof(Topic);
        
        
    }
}
