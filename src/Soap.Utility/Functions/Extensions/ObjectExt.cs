﻿namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Soap.Interfaces.Messages;

    public class SerialiserIds : Enumeration<SerialiserIds>
    {
        public static SerialiserIds ApiBusMessage = Create(nameof(ApiBusMessage), "Bus Messages");

        public static SerialiserIds ClientSideMessageSchemaGeneraton = Create(
            nameof(ClientSideMessageSchemaGeneraton),
            "Schema for JS client");

        public static SerialiserIds JsonDotNetDefault = Create(nameof(JsonDotNetDefault), "Json.NET Defaults");
        
        
        
    }

    internal static class JsonNetSettings
    {
        public static readonly JsonSerializerSettings ApiMessageSerialiserSettings = new JsonSerializerSettings
        {
            DefaultValueHandling =
                DefaultValueHandling
                    .Include, //* this will result in some properties being output that are ignored in JS but i think that's OK
            NullValueHandling =
                NullValueHandling
                    .Include, //* include them for debugging purposes on outgoing messages, but nulls will be changed to undefined in JS constructors as if they were never provided
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            TypeNameHandling =
                TypeNameHandling
                    .Objects, //* ideally could be ignored as already known by JS classes, but may affect object graph structure, checking into this means you may be able to make this NONE
            DateTimeZoneHandling = DateTimeZoneHandling.Utc, ContractResolver = defaultContractResolver
        };

        public static readonly JsonSerializerSettings MessageSchemaSerialiserSettings = new JsonSerializerSettings
        {
            DefaultValueHandling =
                DefaultValueHandling
                    .Include, //* you want all intended properties to be output, false and 0 would otherwise be skipped
            NullValueHandling =
                NullValueHandling.Ignore, //* null however is considered unintended for schema purposes and should be skipped
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            TypeNameHandling = TypeNameHandling.Objects, //* important so we know how to create JS classes
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = defaultContractResolver
        };

        private static readonly DefaultContractResolver defaultContractResolver = new CamelCasePropertyNamesContractResolver();
    }

    public static class ObjectExt
    {
        /// <summary>
        ///     a simpler cast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj) where T : class => (T)obj;

        public static T Clone<T>(this T source) where T : class
        {
            var json = source.ToJson(SerialiserIds.JsonDotNetDefault);
            var assemblyQualifiedName =
                source.GetType()
                      .AssemblyQualifiedName; //* be sure to use the underlying type in case source is assigned to a base class or interface
            var obj = json.FromJson<T>(SerialiserIds.JsonDotNetDefault, assemblyQualifiedName);
            return obj.As<T>();
        }

        /// <summary>
        ///     copies the values of matching properties from one object to another regardless of type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="exclude"></param>
        public static void CopyProperties(this object source, object destination, params string[] exclude)
        {
            // If any this null throw an exception
            if (source == null || destination == null) throw new Exception("Source or/and Destination Objects are null");

            // Getting the Types of the objects
            var typeDest = destination.GetType();
            var typeSrc = source.GetType();

            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead && targetProperty != null && targetProperty.GetSetMethod(true) != null
                                && !targetProperty.GetSetMethod(true).IsPrivate
                                && (targetProperty.GetSetMethod(true).Attributes & MethodAttributes.Static) == 0
                                && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                                && !exclude.Contains(targetProperty.Name)
                          select new
                          {
                              sourceProperty = srcProp, targetProperty
                          };

            // map the properties
            foreach (var props in results)
                props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
        }

        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
            return awaitable.GetAwaiter().GetResult();
        }

        public static bool Is(this object child, Type t) => child.GetType().InheritsOrImplements(t);

        public static To Map<T, To>(this T obj, Func<T, To> map) => map(obj);

        /// <summary>
        ///     perform an operation on any class inline, (e.g. new Object().Op(o => SomeOperationOn(o));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static T Op<T>(this T obj, Action<T> op)
        {
            op(obj);
            return obj;
        }

        public static string ToJson(this object instance, SerialiserIds serialiserId, bool prettyPrint = false)
        {
            /* really important to consider the ramifications of changes in this method.
             If you have historical object that were serialised with one of these settings and you
             change them that would cause potential serious bugs. You should look at adding new items instead,
             but even then you have to consider both serialisation and deserialisation code being updated
             and you also have to consider a code branch to deal with old and new objects based on their serialisationId.
             There is probably a better way but its much better than losing control of the situation entirely. 
             All serialisation should then endeavour to use the ToJson and FromJson methods. Creating these
             choke points will mean that you can easily make changes later without missing any of the many 
             callers of these methods. */

            
            var json = serialiserId switch
            {
                var x when x == SerialiserIds.JsonDotNetDefault => JsonConvert.SerializeObject(
                    instance,
                    prettyPrint ? Formatting.Indented : Formatting.None),
                var x when x == SerialiserIds.ApiBusMessage => JsonConvert.SerializeObject(
                    instance,
                    prettyPrint ? Formatting.Indented : Formatting.None,
                    JsonNetSettings.ApiMessageSerialiserSettings),
                var x when x == SerialiserIds.ClientSideMessageSchemaGeneraton => JsonConvert.SerializeObject(
                    instance,
                    prettyPrint ? Formatting.Indented : Formatting.None,
                    JsonNetSettings.MessageSchemaSerialiserSettings),
                _ => throw new Exception($"Serialiser Id Not Found. Valid values are {SerialiserIds.ListToString()}")
            };
            return json;
        }
    }
}