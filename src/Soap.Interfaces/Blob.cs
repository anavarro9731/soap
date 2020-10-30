namespace Soap.Interfaces
{
    using System;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public class Blob
    {
        public Blob(Guid id, byte[] bytes, BlobType type)
        {
            Guard.Against(id.Equals(Guid.Empty), "Blob ID cannot be empty.");

            Bytes = bytes;
            Type = type;
            Id = id;
        }

        private Blob()
        {
        }

        public byte[] Bytes { get; }

        public Guid Id { get; }

        public BlobType Type { get; }

        public class BlobType
        {
            public BlobType(string typeString, TypeClass typeClass)
            {
                TypeString = typeString;
                TypeClass = typeClass;
            }

            private BlobType()
            {
            }

            public TypeClass TypeClass { get; }

            public string TypeString { get; }
        }

        public class TypeClass : Enumeration<TypeClass>
        {
            public static TypeClass AssemblyQualifiedName = Create(nameof(AssemblyQualifiedName), "Assembly Qualified Name");

            public static TypeClass Mime = Create(nameof(Mime), "MIME / Content-Type");
        }
    }
}