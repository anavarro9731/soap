namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class C107TestForm : UIFormData<C107v1SampleDataTypes>
    {
        protected override List<string> GetPotentialErrors()
        {
            
        }

        protected override void SetInitialValues()
        {
            throw new NotImplementedException();
        }

        protected override void SetSelections()
        {
            throw new NotImplementedException();
        }
    }

    internal class BoolEnumeration : Enumeration<BoolEnumeration>
    {
        private BoolEnumeration No = Create(nameof(No), "No");

        private BoolEnumeration Yes = Create(nameof(Yes), "Yes");
    }

    internal class NullableBoolEnumeration : Enumeration<NullableBoolEnumeration>
    {
        private NullableBoolEnumeration No = Create(nameof(No), "No");

        private NullableBoolEnumeration NotSet = Create(nameof(NotSet), "Not Set");

        private NullableBoolEnumeration Yes = Create(nameof(Yes), "Yes");
    }

    public abstract class UIFormData<T> : ApiEvent where T : ApiCommand
    {
        /* field value types
        text (raw display)
            datestring (datetime)
            guidstring (guid)
            plainstring (string)
            numberstring (long, int, decimal, byte, short)
        toggle (display w/ label)
            enumeration (bool)
        multi-select (display w/ label)
            list<enumeration> (
        */
        protected UIFormData()
        {
            FieldData = GetFieldData(typeof(T)).ToList();

            static IEnumerable<FieldMeta> GetFieldData(Type type)
            {
                foreach (var propertyInfo in typeof(T).GetProperties())
                {
                    //* FRAGILE CachedSchema checks format, but this is still relying on convention for the format of the property
                    var defaultLabel = propertyInfo.Name.Substring(propertyInfo.Name.IndexOf('_'));

                    yield return new FieldMeta
                    {
                        FieldName = propertyInfo.Name,
                        DataType = propertyInfo.PropertyType.Name,
                        Value = GetInitialValues(propertyInfo.PropertyType),
                        SelectableValues = GetInitialSelections(propertyInfo.PropertyType),
                        FieldLabel = defaultLabel
                    };
                }

                static List<Enumeration> GetInitialSelections(Type type)
                {
                    return type switch
                    {
                        _ when type is bool => ((IEnumerable<Enumeration>)BoolEnumeration.GetAll()).ToList(),
                        _ => null
                    };
                }

                static object GetInitialValues(Type type)
                {
                    object @default = type switch
                    {
                        _ when typeof(List<>).IsAssignableFrom(type) => new List<List<FieldMeta>>(),  //* we do allow empty lists
                        _ when !IsSystemType(type) => new List<FieldMeta>(GetFieldData(type)),  //* we don't allow null custom objects
                        _ when  type is bool => false,
                        _ when  type is long? => null,
                        _ when  type is long => 0,
                        _ when  type is decimal? => null,
                        _ when  type is decimal => 0.0,
                        _ when  type is DateTime? => null,
                        _ when  type is Guid? => null,
                        _ when  type is string => null,
                        _ throw new ArgumentException("Type is not a handled type");
                    };

                    static bool IsSystemType(Type type)
                    {
                        char[] SystemTypeChars =
                        {
                            '<', '>', '+'
                        };

                        return type.Namespace == null || type.Namespace.StartsWith("System")
                                                      || type.Name.IndexOfAny(SystemTypeChars) >= 0;
                    }

                    return @default;
                }
            }
        }

        public string CommandName { get; set; } = typeof(T).FullName;

        public List<FieldMeta> FieldData { get; set; }

        public string Validate()
        {
            var errors = GetPotentialErrors();
            var errorString = errors.Aggregate((e1, e2) => $"{e1}{Environment.NewLine}{e2}");
            return string.IsNullOrWhiteSpace(errorString) ? "+" : errorString;
        }

        protected abstract List<string> GetPotentialErrors();

        protected abstract void SetInitialValues();

        protected abstract void SetSelections();

        public class FieldMeta
        {
            public string DataType { get; set; }

            public string FieldLabel { get; set; }

            public string FieldName { get; set; }

            public List<Enumeration> SelectableValues { get; set; }

            public object Value { get; set; }
        }
    }
}
