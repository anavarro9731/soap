namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class C500v1TestForm : UIFormDataCommand<C107v1SampleDataTypes>
    {
        protected override Dictionary<string, List<Enumeration>> UserDefinedOptions()
        {
            return new Dictionary<string, List<Enumeration>>()
            {
                [nameof(C107v1SampleDataTypes.C107_String)] = new List<Enumeration>()
                    { new Enumeration("a", "Apple"), new Enumeration("b", "Banana") },
                
                [nameof(C107v1SampleDataTypes.Address.C107_House)] = new List<Enumeration>()
                    { new Enumeration("lp", "Little Perrugge"), new Enumeration("rc", "Rose Cottage") },
            };
        }

        protected override Dictionary<string, string> UserDefinedInitialValues()
        {
            

            static Dictionary<string, string> ConvertCommandToDefaults<T>(T command) where T : ApiCommand
            {
                //FRAGILE if you add another property to base class it should be excluded too
                foreach (var propertyInfo in typeof(T).GetProperties().Where(t => t.Name != nameof(ApiMessage.Headers)))
                {
                    
                }
            }
        }
        
        
    }

    public abstract class UIFormDataCommand<TMessage> : ApiEvent where TMessage : ApiCommand
    {

        protected UIFormDataCommand()
        {
            FieldData = GetFieldData(typeof(TMessage)).ToList();

            IEnumerable<FieldMeta> GetFieldData(Type type)
            {
                //FRAGILE if you add another property to base class it should be excluded too, no strong link
                foreach (var propertyInfo in type.GetProperties().Where(t => t.Name != nameof(ApiMessage.Headers)))
                {
                    //* FRAGILE CachedSchema checks format, but this is still tying that check to this code in a weak way
                    var defaultLabel = propertyInfo.Name.Substring(propertyInfo.Name.IndexOf('_'));

                    yield return new FieldMeta
                    {
                        FieldName = propertyInfo.Name,
                        DataType = propertyInfo.PropertyType.Name,
                        /* field values can be any of the following which will render to various controls
                            custom object -> panel with the other types                                     
                            (string)date -> datepicker 
                            (string)guid -> guid textbox
                            string -> textbox, textarea or singleselect depending on attributes [Required][AllowCR][IsKey]                            
                            long, decimal -> number textbox
                            bool -> toggle
                            list<string> -> multi-select
                            */
                        Values = InitialValues(propertyInfo), //*
                        Options = AvailableOptions(propertyInfo), //* List<Enumeration> or null
                        FieldLabel = defaultLabel
                    };
                }

                List<Enumeration> AvailableOptions(PropertyInfo property)
                {
                    UserDefinedOptions().TryGetValue(property.Name, out var options);
                    var thisIsAListProperty = typeof(List<>).IsAssignableFrom(property.PropertyType);
                    var theOptionsListIsNullOrEmpty = (options == null || options.Count == 0);
                    if (thisIsAListProperty && theOptionsListIsNullOrEmpty)
                    {
                        throw new ApplicationException($"There are no options available for the {type.Name}.{property.Name} field of the {typeof(TMessage).FullName} command. All multi-select fields require at least one option.");
                    }
                    return options; 
                    
                }

                object InitialValues(PropertyInfo property)
                {

                    /* we need to build out the structure of the command in the field data graph
                     but we only populate the values of the field data graph nodes where the user has provided them. 
                     
                     Matches are made from the user-defined list to the command field based purely on name
                     and so they can be flattened. It's important that messages never have duplicated field names
                     from nested classes for other reasons as well in the system, ??????. 
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
                    
                    UserDefinedInitialValues().TryGetValue(property.Name, out var initalValue)
                    if (userDefinedInitialValues.ContainsKey(property.Name))
                    {
                        return userDefinedInitialValues.GetValueOrDefault(property.Name);
                    }
                    
                    

                    
                    
                    

                    static bool IsSystemType(Type type)
                    {
                        char[] SystemTypeChars =
                        {
                            '<', '>', '+'
                        };

                        return type.Namespace == null || type.Namespace.StartsWith("System")
                                                      || type.Name.IndexOfAny(SystemTypeChars) >= 0;
                    }
                }
            }
        }

        public string CommandName { get; set; } = typeof(TMessage).FullName;

        public List<FieldMeta> FieldData { get; set; }

        protected abstract Dictionary<string, List<Enumeration>> UserDefinedOptions();

        protected abstract Dictionary<string, string> UserDefinedInitialValues();
    }
}
