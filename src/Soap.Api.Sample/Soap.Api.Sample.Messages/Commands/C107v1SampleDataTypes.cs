namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Interfaces.Messages;
    
    public class C107v1SampleDataTypes : ApiMessage
    {
        //* any nullables make for optional fields, null, undefined, not present will be accepted
        //* see JSON.NET serialisation modes for how this is handled by default on .NET side
        
        public bool C107_Boolean { get; set; }
        
        public bool? C107_BooleanOptional { get; set; }

        public override ApiPermission Permission { get; }

        public string C107_String { get; set; }
        
        public string? C107_StringOptional { get; set; }

        public int C107_Integer { get; set; }
        
        public int? C107_IntegerOptional { get; set; }
        
        public short C107_Short { get; set; }
        
        public short? C107_ShortOptional { get; set; }
        
        public byte C107_Byte { get; set; }
        
        public byte? C107_ByteOptional { get; set; }
        
        public long C107_Long { get; set; }

        public long? C107_LongOptional { get; set; }
        
        public decimal C107_Decimal { get; set; }
        
        public decimal? C107_DecimalOptional { get; set; }
        
        public Guid C107_Guid { get; set; }
        
        public Guid? C107_GuidOptional { get; set; }
        
        public DateTime C107_DateTime { get; set; }
        
        public DateTime? C107_DateTimeOptional { get; set; }

        public Enumeration CustomObject { get; set; }
        //* in JS custom child objects cannot be null
        
        public List<Enumeration> ListOfCustomObjects { get; set; }
        //* in JS empty lists will be accepted, null or undefined or not present will not

    }
}
