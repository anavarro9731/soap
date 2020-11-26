namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class C107v1SampleDataTypes : ApiCommand
    {
        //* any nullables make for optional fields, null, undefined, not present will be accepted by client-side in those cases
        //* see JSON.NET serialisation modes for how this is handled by default on .NET side

        public bool C107_Boolean { get; set; }

        public bool? C107_BooleanOptional { get; set; }

        public DateTime C107_DateTime { get; set; }

        public DateTime? C107_DateTimeOptional { get; set; }

        public decimal C107_Decimal { get; set; }

        public decimal? C107_DecimalOptional { get; set; }

        public Guid C107_Guid { get; set; }

        public Guid? C107_GuidOptional { get; set; }

        public long C107_Long { get; set; }

        public long? C107_LongOptional { get; set; }

        public string C107_String { get; set; }

        public string? C107_StringOptional { get; set; }

        public  Address CustomObject { get; set; }

        public class Address
        {
            public string C107_House { get; set; }
            public string C107_Town { get; set; }
            public string C107_PostCode { get; set; }
        }
        
    }
}
