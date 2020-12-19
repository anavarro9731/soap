namespace Soap.PfBase.Messages
{
    using System;
    using Soap.Interfaces.Messages;

    public static class SampleBlobs
    {
        public static BlobMeta Image1 = new BlobMeta
        {
            Id = Guid.Parse("457140C3-933D-4B5A-9D1F-0BDCA8D3FAA9"),
            Name = "soap.jpg"
        };
        
        public static BlobMeta File1 = new BlobMeta
        {
            Id = Guid.Parse("5858C003-CCA0-48CE-B073-381A57A8AB61"),
            Name = "soap.zip"
        };
    }
}
