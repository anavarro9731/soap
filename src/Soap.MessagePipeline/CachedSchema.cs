﻿namespace Soap.MessagePipeline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using CircuitBoard;
    using Namotion.Reflection;
    using Soap.Api.Sample.Messages.Commands.UI;
    using Soap.Config;
    using Soap.Context;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class CachedSchema
    {
        private const string disallowedTypes = " The following types are not allowed in message contracts:"
                                               + "anonymous types, string (use string?), object, [float, double, uint, ulong, ushort, short, sbyte, byte, long (use long?), int (use long?), decimal (use decimal?)], "
                                               + "bool (use bool?), Guid (use Guid?), DateTime (use DateTime?), char, enum (use EnumerationAndFlags) all tuples, objects which implement IDynamicMetaObjectProvider";

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
                    /* FRAGILE ordered by name which puts commands before events. This is important because some events initialise commands in their constructors.
                    if the command is supposed to be invalid, the checks won't have been done on that yet and you'll get unexpected/strange errors */
                    foreach (var type in messageTypes.OrderBy(x => x.Name).ToList())
                    {
                        BuildJsonSchemaForMessageType(type, plainTextBuilder, out var message);

                        /* the only way to get null properties would be if they are ignored
                        that is the logic i think we should keep, null = ignored and you can 
                        use the plaintext version to compare for a difference */
                        var fromMessage = message.ToJson(SerialiserIds.ClientSideMessageSchemaGeneraton, true) + ',';
                        jsonSchemaBuilder.AppendLine(fromMessage);
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

                static void BuildJsonSchemaForMessageType(
                    Type messageType,
                    StringBuilder plainTextBuilder,
                    out ApiMessage message)
                {
                    {
                        Guard.Against(
                            !messageType.InheritsOrImplements(typeof(ApiMessage)),
                            $"Message type {messageType.FullName} does not inherit from ApiMessage");

                        Guard.Against(
                            messageType.GetConstructor(Type.EmptyTypes) == null,
                            $"Message type {messageType.Name} does not have a public parameterless constructor");

                        var messageNameRegex = @"^[EC]\d{3}v[1-9]\d*_[A-Z]+[a-zA-Z0-9]{2,50}$";
                        Guard.Against(
                            !Regex.IsMatch(messageType.Name, messageNameRegex),
                            $"Message type {messageType.Name} does not match regex {messageNameRegex}");

                        PrintMessageName();

                        var propNamesList = new List<string>();

                        var messageWithDefaults = GetWithDefaults(messageType, propNamesList, parentName: messageType.Name);

                        /* allowing duplicates will cause problems client-side in performing find and replace on properties which are meant to be unique based on the class structure
                        may be other reason too that I have forgotten would need to do a test and see if you ever wanted to remove this */
                        Guard.Against(
                            propNamesList.HasDuplicates(),
                            $"Cannot have multiple properties with the same name in the same message. {propNamesList.GetDuplicatesOrNull()?.Aggregate((x, y) => $"{x},{y}")} Check all nested class properties.");

                        message = (ApiMessage)messageWithDefaults;
                    }

                    void PrintMessageName()
                    {
                        var messageTypeName = messageType.ToTypeNameString();
                        var border = string.Empty.PadRight(messageTypeName.Length, '-');
                        plainTextBuilder.AppendLine(border);
                        plainTextBuilder.AppendLine(messageTypeName);
                        plainTextBuilder.AppendLine(border);
                    }

                    object GetWithDefaults(Type aType, List<string> propNamesList, int indent = 0, string parentName = null)
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
                                var errorMessagePrefix = $"Error in message property {parentName}.{property.Name} : ";

                                var propertyNameRegex = @"^[EC]\d{3}_[A-Z]+[a-zA-Z0-9]{2,50}$";
                                Guard.Against(
                                    !Regex.IsMatch(propertyName, propertyNameRegex) && 
                                    !(propertyType == typeof(MessageHeaders)) &&
                                    !(property.DeclaringType.InheritsOrImplements(typeof(EnumerationFlags))) &&
                                    !(property.DeclaringType == typeof(FieldMeta)) &&
                                    !(property.DeclaringType == typeof(Enumeration)),
                                    $"{errorMessagePrefix} property name does not match correct format must match regex {propertyNameRegex}");

                                ProhibitDisallowedTypes(propertyType, errorMessagePrefix, property);

                                var isRequired = property.HasAttribute(typeof(RequiredAttribute)) && !(property.Name == nameof(Enumeration.Active) && property.DeclaringType == typeof(Enumeration));
                                
                                PrintPropertySchema(indent, property, plainTextBuilder, out var totalIndent);
                                
                                property.SetValue(instance, GetDefault(propertyType));

                                if (!property.DeclaringType.InheritsOrImplements(typeof(EnumerationFlags))
                                    && !(property.DeclaringType == typeof(Enumeration)))
                                {
                                    propNamesList.Add(propertyName);    
                                }
                                
                                object GetDefault(Type t)
                                {
                                    return t switch
                                    {
                                        //* handle primitives
                                        _ when t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(List<>) => GetDefaultList(t),  
                                        _ when t == typeof(MessageHeaders) => new MessageHeaders() { GetWithDefaults(typeof(Enumeration), propNamesList, totalIndent, nameof(Enumeration)).As<Enumeration>() }, //don't break this down, we don't break down lists
                                        _ when t == typeof(string) => isRequired ? "string" : string.Empty, //will actually be a string? as checked above but nullable ref types aren't real at run-time
                                        _ when t == typeof(bool?) => isRequired,
                                        _ when t == typeof(long?) => isRequired
                                                                         ? long.MaxValue
                                                                         : long.MinValue, // to say you won't allow fractions
                                        _ when t == typeof(decimal?) => isRequired
                                                                            ? decimal.MaxValue
                                                                            : decimal.MinValue, // to say you will allow fractions
                                        _ when t == typeof(DateTime?) => isRequired ? DateTime.MaxValue : DateTime.MinValue,
                                        _ when t == typeof(Guid?) => isRequired
                                                                         ? Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff")
                                                                         : Guid.Empty,
                                        _ when t == typeof(object) => "optional-primitive", //* this is a very special case only the FieldMeta.InitialValue property and should always be a primitive
                                        //* or break it down till you get to a primitive
                                        _ => GetWithDefaults(t, propNamesList, totalIndent, propertyName)
                                    };

                                    object GetDefaultList(Type typeOfList)
                                    {
                                        var typeParam = typeOfList.GenericTypeArguments.First();
                                        var makeMe = typeof(List<>).MakeGenericType(typeParam);
                                        var l = Activator.CreateInstance(makeMe) as IList;
                                        var @default = GetDefault(typeParam);
                                        l.Add(@default);
                                        return l;
                                    }
                                }

                                static void ProhibitDisallowedTypes(Type t, string errorMessagePrefix, PropertyInfo propertyInfo)
                                {
                                    if (t.IsSystemType())
                                    {
                                        if (t.InheritsOrImplements(typeof(IEnumerable)) && t != typeof(string))
                                        {
                                            Guard.Against(
                                                t.InheritsOrImplements(typeof(ApiCommand)),
                                                "Commands cannot have collections");
                                            
                                            Guard.Against(
                                                !(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)),
                                                $"{errorMessagePrefix} Due to serialisation complexities, the only collections allowed are List<> and it's derivatives. For dictionaries use List<Enumeration> or EnumerationAndFlags");

                                            var typeParam = t.GenericTypeArguments.First();

                                            Guard.Against(
                                                typeParam.InheritsOrImplements(typeof(IEnumerable))
                                                && !typeParam.InheritsOrImplements(typeof(string)),
                                                "Cannot have generic List with IEnumerable type parameters"); //* no lists of lists
                                        } else CheckAllowedSystemPrimitives(t, errorMessagePrefix, propertyInfo);
                                    }
                                    else
                                    {
                                        Guard.Against(
                                            t.InheritsOrImplements(typeof(IEnumerable)) && t != typeof(MessageHeaders),
                                            "Messages cannot have custom types which inherit from IEnumerable, only List<> is allowed and only on Events");
                                        
                                        Type rootDeclaringType = propertyInfo.DeclaringType;

                                        while (rootDeclaringType.DeclaringType != null)
                                        {
                                            rootDeclaringType = rootDeclaringType.DeclaringType;
                                        }

                                        var tIsAllowedNonNestedCustomType =
                                            t.InheritsOrImplements(typeof(EnumerationAndFlags)) || t == typeof(Enumeration) || t == typeof(MessageHeaders);

                                        Guard.Against(
                                            !tIsAllowedNonNestedCustomType
                                            && !rootDeclaringType.InheritsOrImplements(typeof(ApiMessage)),
                                            "All Types used in messages other than primitives, Enumeration and EnumerationFlags which are not defined as nested types within the message are not permitted. This is to stabilise and isolate the contract from future change.");

                                        Guard.Against(
                                            t.GetConstructor(Type.EmptyTypes) == null,
                                            $"{errorMessagePrefix} Cannot use custom types without a public parameterless constructor");
                                    }

                                    static void CheckAllowedSystemPrimitives(Type t, string errPrefix, PropertyInfo property)
                                    {
                                        var isAllowed = t switch
                                        {
                                            _ when t.IsEnum => true,
                                            _ when t.IsAnonymousType() => false,
                                            _ when t == typeof(float) => false,
                                            _ when t == typeof(float?) => false,
                                            _ when t == typeof(double) => false,
                                            _ when t == typeof(double?) => false,
                                            _ when t == typeof(uint) => false,
                                            _ when t == typeof(uint?) => false,
                                            _ when t == typeof(ulong) => false,
                                            _ when t == typeof(ulong?) => false,
                                            _ when t == typeof(ushort) => false,
                                            _ when t == typeof(ushort?) => false,
                                            _ when t == typeof(byte) => false,
                                            _ when t == typeof(byte?) => false,
                                            _ when t == typeof(short) => false,
                                            _ when t == typeof(short?) => false,
                                            _ when t == typeof(int) => false,
                                            _ when t == typeof(int?) => false,
                                            _ when t == typeof(sbyte) => false,
                                            _ when t == typeof(sbyte?) => false,
                                            _ when t == typeof(char) => false,
                                            _ when t == typeof(char?) => false,
                                            _ when t == typeof(object) => property.DeclaringType == typeof(FieldMeta) && property.Name == nameof(FieldMeta.InitialValue) ? true : false,
                                            _ when t.InheritsOrImplements(typeof(ITuple)) => false,
                                            _ when t.InheritsOrImplements(typeof(IDynamicMetaObjectProvider)) => false,

                                            /* because JS can set these to undefined by accident and JSON.NET would serialise to defaults
                                            it's much safer for the default to be null than to accidentally set a value that wasn't provided
                                            and furthermore in the case of Guid and DateTime their defaults are basically errors anyway.
                                            there are similar problems serialising outwards, see notes in UIFormEvent */
                                            _ when t == typeof(Guid) => false,
                                            _ when t == typeof(DateTime) => false,
                                            _ when t == typeof(bool) => false,
                                            _ when t == typeof(long) => false,
                                            _ when t == typeof(decimal) => false,
                                            //nullable strings allowed non-nullables are not, exception for Enumeration class
                                            _ when t == typeof(string) => IsNullableReferenceType(property) || property.DeclaringType == typeof(Enumeration) ? true : false,
                                            /* the only allowable system types in messages */
                                            _ when t == typeof(bool?) => true,
                                            _ when t == typeof(long?) => true,
                                            _ when t == typeof(decimal?) => true,
                                            _ when t == typeof(DateTime?) => true,
                                            _ when t == typeof(Guid?) => true,

                                            _ => throw new ApplicationException(
                                                     $"Unaccounted for system type when preparing message contract: {t.FullName}") //* this will include any System.Nullable<> of any type of than whats listed above
                                        };

                                        Guard.Against(!isAllowed, errPrefix + disallowedTypes);

                                        bool IsNullableReferenceType(PropertyInfo property) =>
                                            property.ToContextualMember().Nullability == Nullability.Nullable;
                                    }
                                }

                                static void PrintPropertySchema(
                                    int indent,
                                    PropertyInfo property,
                                    StringBuilder plainTextBuilder,
                                    out int totalIndent)
                                {
                                    var fieldIndent = new string(' ', indent + 5);
                                    var propertyTypeName = property.PropertyType.ToTypeNameString();

                                    var typeIndentLength = 50 - propertyTypeName.Length;
                                    plainTextBuilder.AppendLine();
                                    plainTextBuilder.Append(fieldIndent + "|-")
                                                    .Append(property.Name.PadRight(typeIndentLength, '-'))
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
