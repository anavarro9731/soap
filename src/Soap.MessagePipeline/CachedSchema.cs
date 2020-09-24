namespace Soap.MessagePipeline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using Soap.Config;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class CachedSchema
    {
        private CachedSchema(string schema)
        {
            Schema = schema;
        }

        public string Schema { get; }

        public static Lazy<CachedSchema> Create(
            ApplicationConfig applicationConfig,
            IEnumerable<ApiMessage> messages)
        {
            var messageTypes = messages.Select(h => h.GetType()).OrderBy(t => t.Name);

            return new Lazy<CachedSchema>(() => new CachedSchema(GetSchemaOutput(applicationConfig, messageTypes)));
        }

        private static string GetMessageSchemaFromMessageTypes(IEnumerable<Type> messageTypes)
        {
            {
                var schema = new StringBuilder();

                foreach (var type in messageTypes.ToList()) BuildSchemaForMessageType(type, schema);

                return schema.ToString();
            }

            void BuildSchemaForMessageType(Type reqType, StringBuilder schema)
            {
                {


                    var depth = 1;

                    var reqTypeName = GetTypeName(reqType);
                    var border = string.Empty.PadRight(reqTypeName.Length, '-');
                    schema.AppendLine(border);
                    schema.AppendLine(reqTypeName);
                    schema.AppendLine(border);
                    
                    if (reqType.InheritsOrImplements(typeof(ApiCommand)))
                    {
                        depth++;
                        schema.AppendLine("  -------");
                        schema.AppendLine("  Command");
                        schema.AppendLine("  -------");
                        AppendObjectToSchema(reqType, depth);
                    }

                    
                    if (reqType.InheritsOrImplements(typeof(ApiEvent)))
                    {
                        schema.AppendLine("  --------");
                        schema.AppendLine("  Event");
                        schema.AppendLine("  --------");
                        AppendObjectToSchema(reqType, depth);
                    }

                    schema.AppendLine();
                }

                void AppendObjectToSchema(Type objectType, int depth = 1)
                {
                    var indentation = string.Empty.PadRight(depth * 2);

                    var typeName = GetTypeFullName(objectType);

                    schema.Append(indentation).AppendLine(typeName);

                    depth++;

                    if (typeof(IEnumerable).IsAssignableFrom(objectType) && objectType.IsGenericType)
                    {
                        objectType = objectType.GenericTypeArguments.First();
                        AppendObjectToSchema(objectType, depth);
                    }
                    else if (objectType.IsEnum)
                    {
                        AppendEnumsToSchema(objectType, depth);
                    }
                    else
                    {
                        AppendObjectPropertiesToSchema(objectType, depth);
                    }
                }

                void AppendEnumsToSchema(Type enumType, int depth = 1)
                {
                    var indentation = string.Empty.PadRight(depth * 2);

                    var baseType = Enum.GetUnderlyingType(enumType);

                    foreach (var enumVal in Enum.GetValues(enumType))
                    {
                        var enumValue = Convert.ChangeType(Enum.Parse(enumType, Convert.ToString(enumVal)), baseType);
                        var enumName = Enum.GetName(enumType, enumVal);
                        schema.Append(indentation).Append(enumName?.PadRight(40)).Append(enumValue);
                        schema.AppendLine();
                    }
                }

                void AppendObjectPropertiesToSchema(Type objectType, int depth = 1)
                {
                    var indentation = string.Empty.PadRight(depth * 2);

                    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(objectType))
                    {
                        var name = descriptor.Name;
                        var type = GetTypeFullName(descriptor.PropertyType);
                        schema.Append(indentation).Append(name.PadRight(40)).Append(type);
                        schema.AppendLine();

                        var nestedType = descriptor.PropertyType;

                        if (typeof(IEnumerable).IsAssignableFrom(descriptor.PropertyType)
                            && descriptor.PropertyType.IsGenericType)
                        {
                            nestedType = descriptor.PropertyType.GenericTypeArguments.First();
                        }

                        if (nestedType.IsSystemType() == false)
                        {
                            AppendObjectToSchema(nestedType, depth + 1);
                        }
                    }
                }

                string GetTypeFullName(Type type)
                {
                    var typeName = type.AsTypeNameString();

                    return typeName;
                }

                string GetTypeName(Type type)
                {
                    var typeName = type.Name;
                    if (type.IsGenericType)
                    {
                        typeName =
                            $"{typeName.Substring(0, typeName.LastIndexOf('`'))}<{string.Join(", ", type.GenericTypeArguments.Select(g => g.Name))}>";
                    }

                    return typeName;
                }
            }
        }

        private static string GetSchemaOutput(
            ApplicationConfig applicationConfig,
            IEnumerable<Type> messageTypes)
        {
            {
                var title = $"API Schema | {applicationConfig.AppFriendlyName} | {applicationConfig.ApplicationVersion}";

                var border = string.Empty.PadRight(title.Length, '=');

                var schemaText = GetMessageSchemaFromMessageTypes(messageTypes);

                if (string.IsNullOrWhiteSpace(schemaText)) schemaText = "No supported API Messages";

                return new StringBuilder().AppendLine(border)
                                          .AppendLine(title)
                                          .AppendLine(border)
                                          .AppendLine()
                                          .AppendLine(schemaText)
                                          .ToString();
            }
        }
    }
}