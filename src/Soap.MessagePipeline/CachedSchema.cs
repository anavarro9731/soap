namespace Soap.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Soap.Config;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class CachedSchema
    {
        private static string _schema;

        public string Schema => _schema;

        public CachedSchema(ApplicationConfig applicationConfig, IEnumerable<ApiMessage> messages)
        {
            if (!string.IsNullOrEmpty(_schema)) return;
            
            var messageTypes = messages.Select(h => h.GetType()).OrderBy(t => t.Name);

            _schema = GetSchemaOutput(applicationConfig, messageTypes);

        }

        private static string GetSchemaOutput(ApplicationConfig applicationConfig, IEnumerable<Type> messageTypes)
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
            
            static string GetMessageSchemaFromMessageTypes(IEnumerable<Type> messageTypes)
        {
            {
                var schemaBuilder = new StringBuilder();

                schemaBuilder.AppendLine("--------");
                schemaBuilder.AppendLine("COMMANDS");
                schemaBuilder.AppendLine("--------");

                foreach (var type in messageTypes.Where(x => x.Name.StartsWith('C')).ToList())
                    BuildSchemaForMessageType(type, schemaBuilder);

                schemaBuilder.AppendLine("--------");
                schemaBuilder.AppendLine(" EVENTS ");
                schemaBuilder.AppendLine("--------");

                foreach (var type in messageTypes.Where(x => x.Name.StartsWith('E')).ToList())
                    BuildSchemaForMessageType(type, schemaBuilder);

                return schemaBuilder.ToString();
            }

            void BuildSchemaForMessageType(Type messageType, StringBuilder schemaBuilder)
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