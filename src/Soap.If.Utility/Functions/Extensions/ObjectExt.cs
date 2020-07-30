namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Soap.Utility.Models;

    public static class ObjectExt
    {
        private static readonly char[] SystemTypeChars =
        {
            '<', '>', '+'
        };

        /// <summary>
        ///     a simpler cast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj) where T : class => obj as T;

        public static string AsTypeNameString(this Type type, bool useFullyQualifiedAssemblyName = false)
        {
            var typesToShortNames = new Dictionary<Type, string>
            {
                {
                    typeof(string), "string"
                },
                {
                    typeof(object), "object"
                },
                {
                    typeof(bool), "bool"
                },
                {
                    typeof(byte), "byte"
                },
                {
                    typeof(char), "char"
                },
                {
                    typeof(decimal), "decimal"
                },
                {
                    typeof(double), "double"
                },
                {
                    typeof(short), "short"
                },
                {
                    typeof(int), "int"
                },
                {
                    typeof(long), "long"
                },
                {
                    typeof(sbyte), "sbyte"
                },
                {
                    typeof(float), "float"
                },
                {
                    typeof(ushort), "ushort"
                },
                {
                    typeof(uint), "uint"
                },
                {
                    typeof(ulong), "ulong"
                },
                {
                    typeof(void), "void"
                }
            };

            if (typesToShortNames.TryGetValue(type, out var nameAsString))
            {
                return nameAsString;
            }

            nameAsString = NameOrFullName(type);

            if (type.IsGenericType)
            {
                var backtick = nameAsString.IndexOf('`');
                if (backtick > 0)
                {
                    nameAsString = nameAsString.Remove(backtick);
                }

                nameAsString += "<";
                var typeParameters = type.GetGenericArguments();
                for (var i = 0; i < typeParameters.Length; i++)
                {
                    var typeParamName = typeParameters[i].AsTypeNameString();
                    nameAsString += i == 0 ? typeParamName : ", " + typeParamName;
                }

                nameAsString += ">";
            }

            if (type.IsArray)
            {
                return type.GetElementType().AsTypeNameString(useFullyQualifiedAssemblyName) + "[]";
            }

            return nameAsString;

            string NameOrFullName(Type t)
            {
                var name = t.Name;
                if (t.IsNested)
                {
                    var tempT = t;

                    do
                    {
                        name = $"{tempT.DeclaringType.Name}+{name}";
                        tempT = t.DeclaringType;
                    }
                    while (tempT != null && tempT.IsNested);
                }

                return useFullyQualifiedAssemblyName ? $"{t.Namespace}.{name}" : name;
            }
        }

        public static T Clone<T>(this T source) => source.ToNewtonsoftJson().FromJson<T>();
        
        

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

        public static T FromJson<T>(this string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings());
            return obj;
        }

        public static T FromJsonToInterface<T>(this string json, string typeName) where T : class
        {
            var obj = JsonConvert.DeserializeObject(json, Type.GetType(typeName), new JsonSerializerSettings()).As<T>();
            return obj.As<T>();
        }

        public static T FromSerialisableObject<T>(this SerialisableObject s) where T : class => s.Deserialise<T>();

        /// <summary>
        ///     get property name from current instance
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type"></param>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(this TObject type, Expression<Func<TObject, object>> propertyRefExpr) =>
            // usage: obj.GetPropertyName(o => o.Member)
            GetPropertyNameCore(propertyRefExpr.Body);

        /// <summary>
        ///     get property name from any class
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> propertyRefExpr) =>
            // usage: Objects.GetPropertyName<SomeClass>(sc => sc.Member)
            GetPropertyNameCore(propertyRefExpr.Body);

        /// <summary>
        ///     get static property name from any class
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetStaticPropertyName<TResult>(Expression<Func<TResult>> expression) =>
            // usage: Objects.GetStaticPropertyName(t => t.StaticProperty)
            GetPropertyNameCore(expression);

        /// <summary>
        ///     checks if a class inherits from or implements a base class/interface.
        ///     Superbly supports generic interfaces and types!
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool InheritsOrImplements(this Type child, Type parent)
        {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType ? child.GetGenericTypeDefinition() : child;

            while (currentChild != typeof(object))
            {
                if (parent == currentChild || //this get a direct match 
                    parent == currentChild.BaseType || //this gets a specific generic impl BaseType<SomeType>
                    HasAnyInterfaces(parent, currentChild))
                    //this child implements any parent interfaces (not sure about specific impl like BaseType<SomeType> requires a test
                {
                    return true;
                }

                currentChild = currentChild.BaseType != null && currentChild.BaseType.IsGenericType
                                   ? currentChild.BaseType.GetGenericTypeDefinition() //this gets a generic impl BaseType<>
                                   : currentChild.BaseType; //this just sets up the next child type

                if (currentChild == null) return false;
            }

            return false;
        }

        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, parameters);
           await awaitable;
           return awaitable.GetAwaiter().GetResult();
        }

        public static bool Is(this object child, Type t) => child.GetType().InheritsOrImplements(t);

        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsSystemType(this Type type) =>
            type.Namespace == null || type.Namespace.StartsWith("System") || type.Name.IndexOfAny(SystemTypeChars) >= 0;

        public static To Map<T, To>(this T obj, Func<T, To> map) => map(obj);

        /// <summary>
        ///     perform an operation on any class inline, (e.g. new Object().Op(o => someoperationon(o));
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

        public static string ToNewtonsoftJson(this object instance, bool prettyPrint = false)
        {
            var json = JsonConvert.SerializeObject(instance, Formatting.Indented, new JsonSerializerSettings());
            return json;
        }

        public static SerialisableObject ToSerialisableObject(this object o) => new SerialisableObject(o);

        private static string GetPropertyNameCore(Expression propertyRefExpr)
        {
            if (propertyRefExpr == null) throw new ArgumentNullException(nameof(propertyRefExpr), "propertyRefExpr is null.");

            var memberExpr = propertyRefExpr as MemberExpression;
            if (memberExpr == null)
            {
                var unaryExpr = propertyRefExpr as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property) return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.", nameof(propertyRefExpr));
        }

        private static bool HasAnyInterfaces(Type parent, Type child)
        {
            return child.GetInterfaces()
                        .Any(
                            childInterface =>
                                {
                                var currentInterface = childInterface.IsGenericType
                                                           ? childInterface.GetGenericTypeDefinition()
                                                           : childInterface;

                                return currentInterface == parent;
                                });
        }

        private static Type ResolveGenericTypeDefinition(Type parent)
        {
            var shouldUseGenericType = true;
            if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent) shouldUseGenericType = false;

            if (parent.IsGenericType && shouldUseGenericType) parent = parent.GetGenericTypeDefinition();

            return parent;
        }
    }
}