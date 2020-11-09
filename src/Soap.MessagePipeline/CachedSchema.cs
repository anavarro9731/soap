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
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class CachedSchema
    {
        private static string _json;

        private static string _schema;

        public CachedSchema(ApplicationConfig applicationConfig, IEnumerable<ApiMessage> messages)
        {
            if (!string.IsNullOrEmpty(_schema)) return;

            var messageTypes = messages.Select(h => h.GetType()).OrderBy(t => t.Name);

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
                GetPlainTextSchemaFromMessageTypes(out plainText);
                GetJsonSchemaFromMessageTypes(out json);
            }

            void GetJsonSchemaFromMessageTypes(out string json)
            {
                {
                    var jsonSchemaBuilder = new StringBuilder();
                    jsonSchemaBuilder.AppendLine("[");
                    foreach (var type in messageTypes.ToList())
                    {
                        BuildJsonSchemaForMessageType(type, out var message);
                        jsonSchemaBuilder.AppendLine(JsonConvert.SerializeObject(message));
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
                                    else if (typeof(List<>).IsAssignableFrom(propertyType))
                                    {
                                        var makeMe = propertyType.MakeGenericType(propertyType);
                                        var l = Activator.CreateInstance(makeMe) as IList;
                                        var typeParam = propertyType.GenericTypeArguments.First();
                                        var @default = GetDefault(typeParam);
                                        l.Add(@default);
                                        property.SetValue(instance, l);
                                    }
                                    else if (typeof(Dictionary<,>).IsAssignableFrom(propertyType))
                                    {
                                        var makeMe = propertyType.MakeGenericType(propertyType);
                                        var d = Activator.CreateInstance(makeMe) as IDictionary;
                                        var keyType = propertyType.GenericTypeArguments.First();
                                        var valueType = propertyType.GenericTypeArguments.Last();
                                        var keyValue = GetDefault(keyType);
                                        var valueValue = GetDefault(valueType);
                                        d.Add(keyValue, valueValue);
                                        property.SetValue(instance, d);
                                    }
                                    else
                                    {
                                        throw new Exception(
                                            $"{errorMessagePrefix} The only collections allowed are List<>, Dictionary<,> and their derivatives");
                                    }
                                }
                                else if (propertyType.IsEnum)
                                {
                                    throw new Exception(
                                        $"{errorMessagePrefix} Cannot have ENUMS in message types, use Enumeration<> instead");
                                }
                                else
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

                                    property.SetValue(instance, GetDefault(propertyType));
                                }

                                object GetDefault(Type type)
                                {
                                    object @default = type switch
                                    {
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

                            if (typeof(IEnumerable<>).IsAssignableFrom(propertyType))
                            {
                                var genericType = propertyType.GenericTypeArguments.First();
                                if (genericType.IsSystemType() == false)
                                {
                                    PrintObjectProperties(genericType, totalIndent);
                                }
                            }
                            else if (propertyType.IsEnum)
                            {
                                throw new Exception("Cannot have ENUMS in message types, use Enumeration<> instead");
                            }
                            else if (propertyType.Name.Contains("AnonymousType"))
                            {
                                throw new Exception("Cannot have anonymous types in message types, use Enumeration<> instead");
                            }
                            else
                            {
                                if (propertyType.IsSystemType())
                                {
                                    var isOk = propertyType switch
                                    {
                                        Type t when t == typeof(float) => false,
                                        Type t when t == typeof(double) => false,
                                        Type t when t == typeof(uint) => false,
                                        Type t when t == typeof(ulong) => false,
                                        Type t when t == typeof(ushort) => false,
                                        Type t when t == typeof(char) => false,
                                        Type t when t == typeof(object) => false,
                                        Type t when t.InheritsOrImplements(typeof(ITuple)) => false,
                                        Type t when t.InheritsOrImplements(typeof(IDynamicMetaObjectProvider)) => false,
                                        _ => true
                                    };
                                    if (!isOk)
                                    {
                                        throw new Exception(
                                            $"Error in message {messageType.AsTypeNameString()} property:{propertyName}. The following types are not allowed in message contracts: float, double, object, uint, ulong, ushort, char, all Tuples, objects which implement IDynamicMetaObjectProvider");
                                    }
                                }
                                else
                                {
                                    PrintObjectProperties(propertyType, totalIndent);
                                }
                            }
                        }

                        schemaBuilder.AppendLine();
                    }
                }
            }
        }
    }
}