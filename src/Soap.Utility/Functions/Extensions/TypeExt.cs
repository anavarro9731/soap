namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class TypeExt
    {
        
        
        /// <summary>
        /// This is like using IsAssignableTo/From (or "is" if you have an instance) but the difference is
        /// those approaches won't work open generics i.e. typeof(I<>).IsAssignableFrom(type) which makes
        /// sense because you can't assign to an open generic. otherwise use "is"
        /// </summary>
        /// <param name="child"></param>
        /// <param name="implementedInterfaceToCheckFor"></param>
        /// <returns></returns>
        public static bool InheritsOrImplements(this Type typeBeingTested, Type implementedInterfaceToCheckFor)
        {
            return implementedInterfaceToCheckFor.IsAssignableFrom(typeBeingTested) || 
                   IterateTypeStack(typeBeingTested, implementedInterfaceToCheckFor);

            static bool IterateTypeStack(Type t, Type implementedInterfaceToCheckFor)
            {
                if (CheckAType(t, implementedInterfaceToCheckFor)) return true;
                
                return t.BaseType != null && IterateTypeStack(t.BaseType, implementedInterfaceToCheckFor);

                static bool CheckAType(Type t, Type implementedInterfaceToCheckFor)
                {
                    return t switch
                    {
                        _ when t == implementedInterfaceToCheckFor => true,
                        _ when CheckForMatchOnInterfaces(t, implementedInterfaceToCheckFor) => true,
                        _ when CheckConstructedTypeForOpenGenericTypeMatch(t, implementedInterfaceToCheckFor) => true,
                        _ => false
                    };
                }
                
                static bool CheckForMatchOnInterfaces(Type t, Type implementedInterfaceToCheckFor)
                {
                    return t.GetInterfaces().Any(i => CheckAType(i,implementedInterfaceToCheckFor));
                }
                
                static bool CheckConstructedTypeForOpenGenericTypeMatch(Type t, Type implementedInterfaceToCheckFor)
                {
                    return t switch
                    {
                        _ when t.IsConstructedGenericType => t.GetGenericTypeDefinition() == implementedInterfaceToCheckFor,
                        _ => false
                    };
                }
            }
        }

        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsSystemType(this Type type)
        {
            char[] SystemTypeChars =
            {
                '<', '>', '+'
            };

            return type.Namespace == null || type.Namespace.StartsWith("System") || type.Name.IndexOfAny(SystemTypeChars) >= 0;
        }

        public static string ToShortAssemblyTypeName(this Type t) => $"{t.FullName}, {t.Assembly.GetName().Name}";

        public static string ToTypeNameString(this Type type, bool useFullyQualifiedAssemblyName = false)
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
                    var typeParamName = typeParameters[i].ToTypeNameString();
                    nameAsString += i == 0 ? typeParamName : ", " + typeParamName;
                }

                nameAsString += ">";
            }

            if (type.IsArray)
            {
                return type.GetElementType().ToTypeNameString(useFullyQualifiedAssemblyName) + "[]";
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
                        tempT = tempT.DeclaringType;
                    }
                    while (tempT != null && tempT.IsNested);
                }

                return useFullyQualifiedAssemblyName ? $"{t.Namespace}.{name}" : name;
            }
        }
    }
}
