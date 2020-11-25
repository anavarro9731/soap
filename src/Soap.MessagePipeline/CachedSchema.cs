namespace Soap.MessagePipeline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using CircuitBoard;
    using Namotion.Reflection;
    using Soap.Config;
    using Soap.Context;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class CachedSchema
    {
        private static string _json;

        private static string _schema;

        public CachedSchema(ApplicationConfig applicationConfig, IEnumerable<ApiMessage> messages)
        {
            if (!string.IsNullOrEmpty(_schema)) return;

            var messageTypes = messages.Select(h => h.GetType()).OrderBy(t => t.Name);

            var unacceptable = messageTypes.GroupBy(x => x.Name).Where(grp => grp.Count() > 1).Select(x => x.Key);
            Guard.Against(
                unacceptable.Any(),
                $"Cannot have 2 messages with the same class name {unacceptable.FirstOrDefault()} (but different namespaces) in the same project");

            GetSchemaOutput(applicationConfig, messageTypes, out var plainText, out var json);

            _schema = plainText;

            _json = json;
        }

        public string AsJson => _json;

        public string PlainTextSchema => _schema;

        private static void GetSchemaOutput(
            ApplicationConfig applicationConfig,
            IEnumerable<Type> messageTypes,
            out string plainText,
            out string json)
        {
            {
                GetJsonSchemaFromMessageTypes(out json, out plainText);
            }

            void GetJsonSchemaFromMessageTypes(out string json, out string text)
            {
                {
                    var jsonSchemaBuilder = new StringBuilder();
                    var plainTextBuilder = new StringBuilder();

                    PrintPlainTextHeader(plainTextBuilder);

                    jsonSchemaBuilder.AppendLine("[");
                    foreach (var type in messageTypes.ToList())
                    {
                        BuildJsonSchemaForMessageType(type, plainTextBuilder, out var message);

                        /* the only way to get null properties would be if they are ignored
                        that is the logic i think we should keep, null = ignored and you can 
                        use the plaintext version to compare for a difference */
                        jsonSchemaBuilder.AppendLine(message.ToJson(SerialiserIds.ClientSideMessageSchemaGeneraton, true) + ',');
                    }

                    var j = jsonSchemaBuilder.ToString();
                    j = j.Remove(j.Length - 3, 2); //remove \r\n
                    j += "]";
                    json = j;

                    text = plainTextBuilder.ToString();
                }

                void PrintPlainTextHeader(StringBuilder stringBuilder)
                {
                    var title = $"API Schema | {applicationConfig.AppFriendlyName} | {applicationConfig.ApplicationVersion}";
                    var border = string.Empty.PadRight(title.Length, '=');
                    var messageCount = $"Message Count: {messageTypes.Count()}";

                    stringBuilder.AppendLine(border);
                    stringBuilder.AppendLine(title);
                    stringBuilder.AppendLine(messageCount);
                    stringBuilder.AppendLine(border);
                }

                static void BuildJsonSchemaForMessageType(Type messageType, StringBuilder plainTextBuilder, out ApiMessage message)
                {
                    void PrintMessageName()
                    {
                        var messageTypeName = messageType.ToTypeNameString();
                        var border = string.Empty.PadRight(messageTypeName.Length, '-');
                        plainTextBuilder.AppendLine(border);
                        plainTextBuilder.AppendLine(messageTypeName);
                        plainTextBuilder.AppendLine(border);
                    }

                    {
                        Guard.Against(
                            !messageType.InheritsOrImplements(typeof(ApiMessage)),
                            $"Message type {messageType.FullName} does not inherit from ApiMessage");

                        Guard.Against(
                            messageType.GetConstructor(Type.EmptyTypes) == null,
                            $"Message type {messageType.Name} does not have a public parameterless constructor");

                        var messageNameRegex = @"^[EC]\d{3}v[1-9]\d*_[A-Z][a-zA-Z0-9]{4,50}$";
                        Guard.Against(
                            !Regex.IsMatch(messageType.Name, messageNameRegex),
                            $"Message type {messageType.FullName} does not match regex {messageNameRegex}");

                        PrintMessageName();

                        var messageWithDefaults = GetWithDefaults(messageType);

                        message = (ApiMessage)messageWithDefaults;
                    }

                    object GetWithDefaults(Type aType, int indent = 0)
                    {
                        var instance = Activator.CreateInstance(aType);

                        /* not sure why this would happen with the current code but its best not to let it slip by.
                         even though the logic below would still be ok that could change in future */
                        Guard.Against(instance == null, $"Could not create instance of {aType}");

                        foreach (var property in aType.GetProperties())
                        {
                            if (property.GetSetMethod() != null && property.CanWrite)
                            {
                                var propertyName = property.Name;
                                var propertyType = property.PropertyType;
                                var errorMessagePrefix =
                                    $"Error in message {messageType.ToTypeNameString()} property {propertyName} : ";

                                PrintPropertySchema(out var totalIndent);

                                var propertyNameRegex = @"^[EC]\d{3}_[A-Z][a-zA-Z0-9]{4,50}$";
                                
                                Guard.Against(
                                    Regex.IsMatch(propertyName, propertyNameRegex),
                                    $"{errorMessagePrefix} property name does not match correct format must match regex {propertyNameRegex}");

                                if (propertyType.InheritsOrImplements(typeof(IEnumerable)))
                                {
                                    if (typeof(string).IsAssignableFrom(propertyType))   //* if its a string
                                    {
                                        var isHardToDetectNullableString =
                                            property.ToContextualMember().Nullability == Nullability.Nullable;
                                        property.SetValue(
                                            instance,
                                            isHardToDetectNullableString ? string.Empty : "string");
                                    }
                                    else if (propertyName == nameof(ApiMessage.Headers)) //* if its the message headers
                                    {
                                        var @default = GetDefault(typeof(Enumeration)).As<Enumeration>();
                                        var headers = new MessageHeaders
                                        {
                                            @default
                                        };
                                        property.SetValue(instance, headers);
                                    }
                                    else if (propertyType.IsGenericType
                                             && propertyType.GetGenericTypeDefinition() == typeof(List<>)) //* if its a list
                                    {
                                        var typeParam = propertyType.GenericTypeArguments.First();
                                        Guard.Against(typeParam.Is(typeof(IEnumerable)) && !typeParam.Is(typeof(string)), "Cannot have generic List with IEnumerable type parameters"); //* no lists of lists
                                        var makeMe = typeof(List<>).MakeGenericType(typeParam);
                                        var l = Activator.CreateInstance(makeMe) as IList;
                                        var @default = GetDefault(typeParam);
                                        l.Add(@default);
                                        property.SetValue(instance, l);
                                    }
                                    else
                                    {
                                        throw new Exception(
                                            $"{errorMessagePrefix} Due to serialisation complexities, the only collections allowed are List<> and it's derivatives. For dictionaries use List<Enumeration> instead.");
                                    }
                                }
                                else
                                {
                                    ProhibitDisallowedTypes(propertyType, errorMessagePrefix);
                                    
                                    property.SetValue(instance, GetDefault(propertyType));
                                }
                                
                                static void ProhibitDisallowedTypes(Type t, string errorMessagePrefix)
                                {
                                    if (t.IsEnum)
                                    {
                                        throw new Exception(
                                            $"{errorMessagePrefix} Cannot have {nameof(Enum)} in message types, use {nameof(Enumeration<Enumeration>)} instead");
                                    }
                                    
                                    if (t.IsSystemType())
                                    {
                                        if (t.InheritsOrImplements(typeof(Nullable<>)))
                                        {
                                            var underlying = t.GenericTypeArguments.Last();
                                            IsBadPrimitive(underlying, errorMessagePrefix);    
                                        }
                                        else
                                        {
                                            IsBadPrimitive(t, errorMessagePrefix);
                                        }
                                    }
                                    else
                                    {
                                        Guard.Against(
                                            t.GetConstructor(Type.EmptyTypes) == null,
                                            $"{errorMessagePrefix} Cannot use custom types without a public parameterless constructor");
                                    }
                                        
                                    static void IsBadPrimitive(Type t, string errPrefix)
                                    {

                                        var isBad = t switch
                                        {
                                            _ when t.IsAnonymousType() => true,
                                            _ when t == typeof(float) => true,
                                            _ when t == typeof(double) => true,
                                            _ when t == typeof(uint) => true,
                                            _ when t == typeof(ulong) => true,
                                            _ when t == typeof(ushort) => true,
                                            _ when t == typeof(byte) => true,
                                            _ when t == typeof(short) => true,
                                            _ when t == typeof(int) => true,
                                            _ when t == typeof(sbyte) => true,
                                            _ when t == typeof(char) => true,
                                            _ when t == typeof(object) => true,
                                            _ when t.InheritsOrImplements(typeof(ITuple)) => true,
                                            _ when t.InheritsOrImplements(typeof(IDynamicMetaObjectProvider)) => true,
                                            
                                            /* because JS can set these to undefined by accident and JSON.NET would serialise to defaults
                                            it's much safer for the default to be null than to accidentally set a value that wasn't provided
                                            and furthermore in the case of Guid and DateTime their defaults are basically errors anyway */
                                            _ when t == typeof(Guid) => true, 
                                            _ when t == typeof(DateTime) => true,                                             
                                            _ when t == typeof(bool) => true,
                                            _ when t == typeof(long) => true,
                                            _ when t == typeof(decimal) => true,
                                            
                                            /* the only allowable primitives */
                                            _ when t == typeof(bool?) => false,
                                            _ when t == typeof(long?) => false,
                                            _ when t == typeof(decimal?) => false,
                                            _ when t == typeof(DateTime?) => false,
                                            _ when t == typeof(Guid?) => false,
                                            //string and string? handled above with enumerables
                                            _ => throw new Exception($"Unaccounted for system type when preparing message contract: {t.FullName}")
                                        };

                                        Guard.Against(isBad, $"{errPrefix} The following types are not allowed in message contracts:" +  
                                            "anonymous types, object, [float, double, uint, ulong, ushort, short, sbyte, byte, long (use long?), int (use long?), decimal (use decimal?)], " +
                                            "bool (use bool?), Guid (use Guid?), DateTime (use DateTime?), char, all tuples, objects which implement IDynamicMetaObjectProvider");
                                    }
                                }

                                object GetDefault(Type type)
                                {
                                    return type switch
                                    {
                                        Type t when t == typeof(bool?) => false,
                                        Type t when t == typeof(long?) => null,
                                        Type t when t == typeof(decimal?) => null,
                                        Type t when t == typeof(DateTime?) => null,
                                        Type t when t == typeof(Guid?) => null,
                                        //strings handled above with enumerables
                                        _ => GetWithDefaults(type, totalIndent)
                                    };
                                }

                                void PrintPropertySchema(out int totalIndent)
                                {
                                    var fieldIndent = new string(' ', indent + 5);
                                    var propertyTypeName = property.PropertyType.ToTypeNameString();

                                    var typeIndentLength = 50 - propertyTypeName.Length;
                                    plainTextBuilder.AppendLine();
                                    plainTextBuilder.Append(fieldIndent + "|-")
                                                    .Append(propertyName.PadRight(typeIndentLength, '-'))
                                                    .Append(propertyTypeName);
                                    plainTextBuilder.AppendLine();

                                    totalIndent = fieldIndent.Length + typeIndentLength + 2;
                                }
                            }
                        }

                        return instance;
                    }
                }
            }
        }
    }
}
