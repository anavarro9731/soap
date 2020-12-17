﻿namespace Soap.Pf.MessageContractsBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands.UI;
    using Soap.Interfaces.Messages;

    public abstract class UIFormDataEvent : ApiEvent
    {
        public string E000_CommandName { get; set; }

        public Guid? E000_CommandBlobId { get; set; }
        
        public string E000_SasStorageTokenForCommand { get; set; }
        
        public List<FieldMeta> E000_FieldData { get; set; }

        /*
           we need to build out the structure of the command in the field data graph
           but we only populate the values of the field data graph nodes where the user has provided them. 
           
           Matches are made from the user-defined list to the command field based purely on name
           and so they can be flattened. It's important that messages never have duplicated field names
           from nested classes for other reasons as well in the system. See CachedSchema for check. 
           Because all classes are nested this can be controlled rather easily. This is validated when
           building the schema so it's already assured here.
           
           values not explicitly provided should be null/default so that new forms will not default to a value
           the user didn't select. this is an issue where the default of structs represents
           an actual choice, e.g. false, 0 and also where the default is non-sense e.g.
           DateTimes and Guids. You could handle the non-sense values by 'interpreting' them
           as defaults but for the ambiguous cases you would have to accept them.  
           
           The second thing you have to contend with is how to tell the form that a field is
           required or not, while you can ensure this with an API validator in a toaster on
           submit, you need a way to indicate this to user when the form is rendered as that
           is just not acceptable in 2020 to find out later. Moreover you are relying on toaster
           for all other validation, rather then field by field you are already taking a UI hit.
      
           So as far as I can see it there are two options for satisfying both needs:
           
           1. use nullable fields to represent required/not required and make all the front-end
           controls aware of non-nullable defaults to 'treat them as null' so that the values
           are not rendered into the controls and to make validation in both the form and the class 
           aware of these defaults and to block them i.e. 'treat them as not provided' 
           
           2. use nullable fields only so defaults are always null for any type, as it is 
           in JS itself, and then determine required/not required in other form, for example
           from an attribute. this would mean controls can always count on null defaults and
           that any value is user defined. it would mean that you still need to communicate required
           to the front-end based on special values but these are determined by an attribute and 
           not by the nullable flag on the property.
           
           I have chosen 2 since I hate having to work around the exceptions of the default values
           of value types.
                                
            */

        
        
        public void SetProperties(string sasTokenForCommand, Guid idOfCommand)
        {
            {
                E000_CommandName = ToShortAssemblyTypeName(UserDefinedValues().GetType());
                
                var fieldData = new List<FieldMeta>();

                BuildFieldAndObjectStructureData(
                    UserDefinedValues().GetType(),
                    UserDefinedValues(),
                    string.Empty,
                    fieldData,
                    E000_CommandName);

                E000_FieldData = fieldData;
                E000_SasStorageTokenForCommand = sasTokenForCommand;
                E000_CommandBlobId = idOfCommand;
            }
            
            static string ToShortAssemblyTypeName(Type t) => $"{t.FullName}, {t.Assembly.GetName().Name}";

            static void BuildFieldAndObjectStructureData(
                Type type,
                object instance,
                string typePath,
                List<FieldMeta> fieldData,
                string commandName)
            {
                {
                    //* FRAGILE if you add another property like Headers to ApiMessage base class it should be excluded too, no strong link
                    foreach (var validProperty in type.GetProperties().Where(t => t.Name != nameof(Headers)))
                    {
                        /* You can be confident that the ApiCommand returned from UserDefinedValues does not have any invalid properties
                        since it would already be checked by cached schema, which processes commands before events (which this is) ensuring the command
                        you are parsings' schema has already been validated */

                        if (PropertyTypeIsMessagePrimitive(validProperty))
                        {
                            CreateFieldMetaObjectFromProperty(validProperty, typePath, instance, commandName, out var fieldMeta);

                            CheckFieldMetaToAvoidPassingEmptyEnumerations(fieldMeta, commandName, validProperty);

                            fieldData.Add(fieldMeta);

                        }
                        else
                        {
                            //* recurse
                            BuildFieldAndObjectStructureData(
                                validProperty.PropertyType,
                                validProperty.GetValue(instance)
                                ?? Activator.CreateInstance(
                                    validProperty
                                        .PropertyType), //* all custom types should have parameterless constructors, and not be nullables see CachedSchema check
                                GetPropertyPath(validProperty, typePath),
                                fieldData,
                                commandName); //* will flatten property list
                        }
                    }

                    static void CreateFieldMetaObjectFromProperty(
                        PropertyInfo validProperty,
                        string typePath,
                        object instance,
                        string commandName,
                        out FieldMeta fieldMeta)
                    {
                        GetADefaultPropertyLabel(validProperty, out var defaultLabel);

                        fieldMeta = new FieldMeta
                        {
                            Name = GetPropertyPath(validProperty, typePath),
                            Required = HasAttribute(validProperty, typeof(RequiredAttribute)),

                            Caption = HasAttribute(validProperty, typeof(RequiredAttribute)) ? "required" : string.Empty,
                            Label = HasAttribute(validProperty, typeof(LabelAttribute))
                                        ? validProperty.GetCustomAttribute<LabelAttribute>().Label
                                        : defaultLabel,
                            DataType = validProperty.PropertyType switch
                            {
                                var t when t == typeof(DateTime?) => "datetime", // -> datepicker
                                var t when t == typeof(Guid?) => "guid", // -> guid textbox
                                var t when t == typeof(string) => HasAttribute(validProperty, typeof(MultiLineAttribute))
                                                                      ? "multilinestring"
                                                                      : "string", // -> textbox or textarea
                                var t when t == typeof(BlobMeta) => HasAttribute(validProperty, typeof(IsImageAttribute))
                                                                          ? "image"
                                                                          : "file", //-> fileupload //* we don't break down blobs, like other custom objects, they are considered primitives for form-building
                                var t when t == typeof(long?) => "number", // -> number textbox
                                var t when t == typeof(decimal?) => "number", // -> number textbox
                                var t when t == typeof(bool?) => "boolean", // -> toggle
                                var t when t == typeof(EnumerationAndFlags)
                                    => //* we don't break down enumerations, like other custom objects, they are considered primitives for form-building
                                    "enumeration", //-> select (single or multi) 
                                _ => throw new ApplicationException(
                                         $"Unexpected Data Type {validProperty.PropertyType} on property {validProperty.Name} in command {commandName}. Please validate schema.")
                            },
                            InitialValue = validProperty.GetValue(instance)
                        };
                    }

                    static void CheckFieldMetaToAvoidPassingEmptyEnumerations(
                        FieldMeta fieldMeta,
                        string commandName,
                        PropertyInfo validProperty)
                    {
                        if (fieldMeta.DataType == "enumeration" && fieldMeta.Required.GetValueOrDefault()
                                                                && (fieldMeta.InitialValue == null
                                                                    || ((EnumerationAndFlags)fieldMeta.InitialValue)
                                                                       .AllEnumerations.Count == 0))
                        {
                            throw new ApplicationException(
                                $"Cannot present enumeration for property {validProperty.Name} in command {commandName} because the list of options is null or empty but the field is required.");
                        }
                    }

                    //* FRAGILE CachedSchema checks format, but this is still tying that check to this code in a weak way
                    static void GetADefaultPropertyLabel(PropertyInfo validProperty, out string defaultLabel) =>
                        defaultLabel = validProperty.Name.Substring(validProperty.Name.IndexOf('_') + 1);



                    static string GetPropertyPath(PropertyInfo validProperty, string typePath) =>
                        !string.IsNullOrWhiteSpace(typePath) ? typePath + '.' + CamelCase(validProperty.Name) : CamelCase(validProperty.Name);

                    static string CamelCase(string s) => char.ToLower(s[0]) + s.Substring(1);
                    
                    static bool HasAttribute(PropertyInfo prop, Type attributeType)
                    {
                        var att = prop.GetCustomAttributes(attributeType).Where(x => x.GetType() == attributeType);
                        return att != null && att.Any();
                    }

                    static bool PropertyTypeIsMessagePrimitive(PropertyInfo propertyInfo)
                    {
                        static bool IsSystemType(Type type)
                        {
                            char[] SystemTypeChars =
                            {
                                '<', '>', '+'
                            };

                            return type.Namespace == null || type.Namespace.StartsWith("System")
                                                          || type.Name.IndexOfAny(SystemTypeChars) >= 0;
                        }

                        return IsSystemType(propertyInfo.PropertyType)
                               || typeof(EnumerationFlags).IsAssignableFrom(propertyInfo.PropertyType)
                               || propertyInfo.PropertyType == typeof(BlobMeta);
                    }
                }
            }
        }

        protected abstract ApiCommand UserDefinedValues();
    }
}