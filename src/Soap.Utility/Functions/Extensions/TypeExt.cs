namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class TypeExt
    {
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

            static bool HasAnyInterfaces(Type parent, Type child)
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

            static Type ResolveGenericTypeDefinition(Type parent)
            {
                var shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

                if (parent.IsGenericType && shouldUseGenericType) parent = parent.GetGenericTypeDefinition();

                return parent;
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