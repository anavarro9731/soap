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
    using Namotion.Reflection;
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

                void BuildJsonSchemaForMessageType(Type messageType, StringBuilder plainTextBuilder, out ApiMessage message)
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

                                 PrintPropertySchema(out int totalIndent);

                                if (propertyType.InheritsOrImplements(typeof(IEnumerable)))
                                {
                                    if (typeof(string).IsAssignableFrom(propertyType))
                                    {
                                        var isHardToDetectNullableString =
                                            property.ToContextualMember().Nullability == Nullability.Nullable;
                                        property.SetValue(
                                            instance,
                                            isHardToDetectNullableString ? string.Empty : GetDefault(propertyType));
                                    } else if (propertyName == nameof(ApiMessage.Headers))
                                    {
                                        var @default = GetDefault(typeof(Enumeration)).As<Enumeration>();
                                        var headers = new MessageHeaders
                                        {
                                            @default
                                        };
                                        property.SetValue(instance,headers);
                                        
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
                                else if (propertyType.IsEnum)
                                {
                                    throw new Exception(
                                        $"{errorMessagePrefix} Cannot have {nameof(Enum)} in message types, use {nameof(Enumeration<Enumeration>)} instead");
                                }
                                else
                                {
                                    property.SetValue(instance, GetDefault(propertyType));
                                }

                                object GetDefault(Type type)
                                {
                                    ProhibitDisallowedTypes(type);

                                    IsNullable(type);

                                    object @default = type switch
                                    {
                                        Type t when t == typeof(bool) => true,
                                        Type t when t == typeof(bool?) => false,
                                        Type t when t == typeof(sbyte?) => sbyte.MinValue, //-128
                                        Type t when t == typeof(sbyte) => sbyte.MaxValue, //127
                                        Type t when t == typeof(byte?) => byte.MinValue, //0
                                        Type t when t == typeof(byte) => byte.MaxValue, //255
                                        Type t when t == typeof(short?) => short.MinValue, //-32768
                                        Type t when t == typeof(short) => short.MaxValue, //32767
                                        Type t when t == typeof(int?) => int.MinValue, //-2147483648
                                        Type t when t == typeof(int) => int.MaxValue, //2147483647
                                        Type t when t == typeof(long?) => long.MinValue, //-9223372036854775808
                                        Type t when t == typeof(long) => long.MaxValue, //9223372036854775807
                                        Type t when t == typeof(decimal) => decimal.MinValue, //-79228162514264337593543950335.0
                                        Type t when t == typeof(decimal?) => decimal.MaxValue, //79228162514264337593543950335.0
                                        Type t when t == typeof(DateTime) => DateTime.MaxValue, //9999-12-31T23:59:59.9999999Z
                                        Type t when t == typeof(DateTime?) => DateTime.MinValue, //0001-01-01T00:00:00Z
                                        Type t when t == typeof(Guid?) => Guid.Empty, //00000000-0000-0000-0000-000000000000
                                        Type t when t == typeof(Guid) => Guid.NewGuid(),
                                        Type t when t == typeof(string) => "string",
                                        _ => null
                                    };

                                    if (@default != null) return @default;

                                    Guard.Against(
                                        propertyType.GetConstructor(Type.EmptyTypes) == null,
                                        $"{errorMessagePrefix} Cannot use types without a public parameterless constructor");

                                    return GetWithDefaults(type, totalIndent);

                                    void IsNullable(Type t)
                                    {
                                        if (type.InheritsOrImplements(typeof(Nullable<>)))
                                        {
                                            var underlying = propertyType.GenericTypeArguments.Last();
                                            ProhibitDisallowedTypes(underlying);
                                        }
                                    }

                                    void ProhibitDisallowedTypes(Type propertyType)
                                    {
                                        var isBad = propertyType switch
                                        {
                                            Type t when t.IsAnonymousType() => true,
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
                                }

                                void PrintPropertySchema(out int totalIndent)
                                {
                                    var fieldIndent = new string(' ', indent + 5);
                                    var propertyTypeName = property.PropertyType.ToTypeNameString();

                                    var typeIndentLength = 50 - propertyTypeName.Length;
                                    plainTextBuilder.AppendLine();
                                    plainTextBuilder.Append(fieldIndent + "|-").Append(propertyName.PadRight(typeIndentLength, '-')).Append(propertyTypeName);
                                    plainTextBuilder.AppendLine();
                                    
                                    totalIndent= fieldIndent.Length + typeIndentLength + 2;
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