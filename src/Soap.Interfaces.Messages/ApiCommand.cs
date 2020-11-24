namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /* 
    NEVER add any logic to these classes, or you may risk conflicts between versions of message 
    contract assemblies. Use headers to implement variables logic. If you are going to use 
    static classes as a base interface for messages then you must make sure you never add any logic
    which is not backwards compatible. 
     */

    public abstract class ApiCommand : ApiMessage
    {
    }

    public abstract class UIFormData : ApiEvent
    {
        public ApiCommand Command { get; set; }
        public List<FieldMeta> FieldData { get; set; }

        public class FieldMeta
        {
            public string FieldName { get; set; }
            public string InitialValue { get; set; }
            public List<string> SelectableValues { get; set; }
            public string FieldLabel { get; set; }
        }

        protected abstract List<string> GetPotentialErrors();

        public string Validate()
        {
            var errors = GetPotentialErrors();
            var errorString = errors.Aggregate((e1, e2) => $"{e1}{Environment.NewLine}{e2}");
            return string.IsNullOrWhiteSpace(errorString) ? "+" : errorString;
        }
        
        
    }
}
