namespace Soap.MessagePipeline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Newtonsoft.Json;
    using Soap.Config;
    using Soap.Context;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
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
                GetJsonSchemaFromMessageTypes(out json);
                /* NOTE: GetJsonSchemaFromMessageTypes this has the checks so when we run plaintext builder we assume all types are valid
                 and we only concern ourselves with types as muchas is needed to print them correctly */
                GetPlainTextSchemaFromMessageTypes(out plainText);
            }

            void GetJsonSchemaFromMessageTypes(out string json)
            {
                {
                    var jsonSchemaBuilder = new StringBuilder();
                    jsonSchemaBuilder.AppendLine("[");
                    foreach (var type in messageTypes.ToList())
                    {
                        BuildJsonSchemaForMessageType(type, out var message);
                        message.Headers.SetHeadersOnSchemaModelMessage(message);

                        
                        /* the only way to get null properties would be if they are ignored
                        that is the logic i think we should keep, null = ignored and you can 
                        use the plaintext version to compare for a difference */
                        jsonSchemaBuilder.AppendLine(
                            JsonConvert.SerializeObject(
                                message,
                                Formatting.Indented,
                                JsonNetSettings.MessageSchemaSerialiserSettings) + ',');
                    }

                    jsonSchemaBuilder.AppendLine("]");
                    json = jsonSchemaBuilder.ToString();
                }

                void BuildJsonSchemaForMessageType(Type messageType, out ApiMessage message)
                {
                    {
                        Guard.Against(
                            !messageType.InheritsOrImplements(typeof(ApiMessage)),
                            $"Message type {messageType.FullName} does not inherit from ApiMessage");

                        Guard.Against(
                            messageType.GetConstructor(Type.EmptyTypes) == null,
                            $"Message type {messageType.Name} does not have a public parameterless constructor");

                        var messageWithDefaults = GetWithDefaults(messageType);

                        message = (ApiMessage)messageWithDefaults;
                    }

                    object GetWithDefaults(Type aType)
                    {
                        var instance = Activator.CreateInstance(aType);
                        
                        /* not sure why this would happen with the current code but its best not to let it slip by.
                         even though the logic below would still be ok that could change in future */
                        Guard.Against(instance == null, $"Could not create instance of {aType}"); 

                        foreach (var property in aType.GetProperties())
                            if (property.GetSetMethod() != null && property.CanWrite)
                            {
                                var propertyName = property.Name;
                                var propertyType = property.PropertyType;
                                var errorMessagePrefix =
                                    $"Error in message {messageType.AsTypeNameString()} property {propertyName} : ";

                                if (propertyType.InheritsOrImplements(typeof(IEnumerable)))
                                {
                                    if (typeof(string).IsAssignableFrom(propertyType))
                                    {
                                        property.SetValue(instance, GetDefault(typeof(string)));
                                    }
                                    else if (propertyType.IsGenericType
                                             && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        var typeParam = propertyType.GenericTypeArguments.First();
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
                                else if (propertyType.InheritsOrImplements(typeof(Nullable<>)))
                                {
                                    var valueType = propertyType.GenericTypeArguments.Last();
                                    ProhibitDisallowedTypes(valueType);
                                    var @default = GetDefault(valueType);
                                    property.SetValue(instance, @default);
                                }
                                else if (propertyType.IsEnum)
                                {
                                    throw new Exception(
                                        $"{errorMessagePrefix} Cannot have {nameof(Enum)} in message types, use {nameof(Enumeration<Enumeration>)} instead");
                                }
                                else
                                {
                                    ProhibitDisallowedTypes(propertyType);
                                    property.SetValue(instance, GetDefault(propertyType));
                                }

                                void ProhibitDisallowedTypes(Type propertyType)
                                {
                                    var isBad = propertyType switch
                                    {
                                        Type t when t.Name.Contains("AnonymousType") => true,
                                        Type t when t == typeof(float) => true,
                                        Type t when t == typeof(double) => true,
                                        Type t when t == typeof(uint) => true,
                                        Type t when t == typeof(ulong) => true,
                                        Type t when t == typeof(ushort) => true,
                                        Type t when t == typeof(char) => true,
                                        Type t when t == typeof(object) => true,
                                        Type t when t.InheritsOrImplements(typeof(ITuple)) => true,
                                        Type t when t.InheritsOrImplements(typeof(IDynamicMetaObjectProvider)) => true,
                                        _ => false
                                    };

                                    Guard.Against(
                                        isBad,
                                        $"{errorMessagePrefix} The following types are not allowed in message contracts: anonymous types, float, double, object, uint, ulong, ushort, char, all Tuples, objects which implement IDynamicMetaObjectProvider");
                                }

                                object GetDefault(Type type)
                                {
                                    object @default = type switch
                                    {
                                        Type t when t == typeof(bool) => true,
                                        Type t when t == typeof(sbyte) => sbyte.MaxValue,
                                        Type t when t == typeof(byte) => byte.MaxValue,
                                        Type t when t == typeof(short) => short.MaxValue,
                                        Type t when t == typeof(int) => int.MaxValue,
                                        Type t when t == typeof(long) => long.MaxValue,
                                        Type t when t == typeof(decimal) => decimal.MaxValue,
                                        Type t when t == typeof(DateTime) => DateTime.MaxValue,
                                        Type t when t == typeof(Guid) => Guid.Empty,
                                        Type t when t == typeof(string) => "string",
                                        _ => null
                                    };

                                    if (@default != null) return @default;

                                    Guard.Against(
                                        propertyType.GetConstructor(Type.EmptyTypes) == null,
                                        $"{errorMessagePrefix} Cannot use types without a public parameterless constructor");

                                    return GetWithDefaults(type);
                                }
                            }

                        return instance;
                    }
                }
            }

            void GetPlainTextSchemaFromMessageTypes(out string plainText)
            {
                var plainTextBuilder = new StringBuilder();

                var title = $"API Schema | {applicationConfig.AppFriendlyName} | {applicationConfig.ApplicationVersion}";
                var border = string.Empty.PadRight(title.Length, '=');
                var messageCount = $"Message Count: {messageTypes.Count()}";

                plainTextBuilder.AppendLine(border);
                plainTextBuilder.AppendLine(title);
                plainTextBuilder.AppendLine(messageCount);
                plainTextBuilder.AppendLine(border);

                plainTextBuilder.AppendLine("--------");
                plainTextBuilder.AppendLine("COMMANDS");
                plainTextBuilder.AppendLine("--------");

                foreach (var type in messageTypes.Where(x => x.Name.StartsWith('C')).ToList())
                    BuildPlainSchemaForMessageType(type, plainTextBuilder);

                plainTextBuilder.AppendLine("--------");
                plainTextBuilder.AppendLine(" EVENTS ");
                plainTextBuilder.AppendLine("--------");

                foreach (var type in messageTypes.Where(x => x.Name.StartsWith('E')).ToList())
                    BuildPlainSchemaForMessageType(type, plainTextBuilder);

                plainText = plainTextBuilder.ToString();

                void BuildPlainSchemaForMessageType(Type messageType, StringBuilder schemaBuilder)
                {
                    {
                        schemaBuilder.AppendLine();

                        PrintMessageName();

                        if (messageType.InheritsOrImplements(typeof(ApiCommand)))
                        {
                            PrintObjectProperties(messageType);
                        }

                        if (messageType.InheritsOrImplements(typeof(ApiEvent)))
                        {
                            PrintObjectProperties(messageType);
                        }

                        schemaBuilder.AppendLine();
                    }

                    void PrintMessageName()
                    {
                        var messageTypeName = messageType.AsTypeNameString();
                        var border = string.Empty.PadRight(messageTypeName.Length, '-');
                        schemaBuilder.AppendLine(border);
                        schemaBuilder.AppendLine(messageTypeName);
                        schemaBuilder.AppendLine(border);
                    }

                    void PrintObjectProperties(Type objectType, int indent = 0)
                    {
                        var fieldIndent = new string(' ', indent + 5);

                        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(objectType))
                        {
                            var propertyName = property.Name;
                            var propertyTypeName = property.PropertyType.AsTypeNameString();
                            var propertyType = property.PropertyType;

                            var typeIndentLength = 75 - propertyTypeName.Length;
                            schemaBuilder.Append(fieldIndent + "|-")
                                         .Append(propertyName.PadRight(typeIndentLength, '-'))
                                         .Append(propertyTypeName);
                            schemaBuilder.AppendLine();

                            var totalIndent = fieldIndent.Length + typeIndentLength + 2;

                            PrintPropertyMetaIfNeeded();

                            void PrintPropertyMetaIfNeeded()
                            {
                                if (typeof(IEnumerable<>).IsAssignableFrom(propertyType) || typeof(Nullable<>).IsAssignableFrom(propertyType))
                                {
                                    var genericType = propertyType.GenericTypeArguments.First();
                                    if (genericType.IsSystemType() == false)
                                    {
                                        //* print custom type under enumerable
                                        PrintObjectProperties(genericType, totalIndent);
                                    }
                                }
                                else if (propertyType.IsSystemType() == false)
                                {
                                    PrintObjectProperties(propertyType, totalIndent);
                                }
                            }
                        }
                    }

                    schemaBuilder.AppendLine();
                }
            }
        }
    }
}