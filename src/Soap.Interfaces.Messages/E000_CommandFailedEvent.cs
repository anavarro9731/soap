namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     It's a very important interface because it is shared amongst services.
    ///     so it is of absolute importance that the structure remain simple, and backwards and forwards compatible or services
    ///     running on different an older version of the platform would send messages that deserialise into something not
    ///     understandable
    ///     by the receiver.
    ///     If you change this interface, [to v2] you will need to publish both versions of the error to catch
    ///     older clients, or you will need to scrap versioning altogether and make that this event is ALWAYS
    ///     backwards compatible, but if you have a newer soap frontend talking to an older backend, you will need to make
    ///     sure it can handle all versions as well.
    /// </summary>
    public class E001v1_MessageFailed : ApiEvent
    {
        public List<Guid> E001_ErrorCodes { get; set; }

        public string E001_ErrorMessage { get; set; }

        public Guid? E001_MessageId { get; set; }

        public string E001_MessageTypeName { get; set; } 

        public StatefulProcessId? E001_StatefulProcessId { get; set; }

        public override void Validate()
        {
        }
    }
}
