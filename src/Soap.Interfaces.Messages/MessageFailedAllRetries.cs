namespace Soap.Interfaces.Messages
{
    /// <summary>
    /// Sent to queue of OWNING service when processing failed to handle the message max number of times
    /// so that the sender can take compensating action if need be to ensure integrity of its state
    ///
    /// It's a very important interface because it is shared amongst services. The receiver sends back to the sender
    /// so it is of absolute importance that the structure remain simple, and backwards and forwards compatible or services
    /// running on different  an older version of the platform would send messages that deserialise into something not understandable
    /// by the receiver.
    ///
    /// The big issue is to avoid conflicting versions of dependent assemblies in your project when your service inherits from
    /// multiple other services' contract assemblies. Because the project which imports the messages is compiled into one
    /// there is no benefits to creating a derived version of this message in each messages project as you tried once before,
    /// and you also want to avoid if possible including a serialiser assembly since that may end of causing an annoying version
    /// conflict at build time, even if it does work, better to rely on the json standard which is not changing as the
    /// whole contract and allow for different possible versions of the serialiser in the soap core of different services.
    /// </summary>
    /// <typeparam name="TFailedMessage"></typeparam>
    
    public class MessageFailedAllRetries : ApiCommand
    {
        /* FROM DOCS:
         * https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype?view=net-5.0#System_Type_GetType_System_String_
         * The assembly-qualified name of the type to get. See AssemblyQualifiedName. *BUT*
         * If the type is in the currently executing assembly or in mscorlib.dll/System.Private.CoreLib.dll, it is sufficient to supply the type name qualified by its namespace (i.e. type.FullName)
         */
        public string TypeName { get; set; } //* of format Soap.Whatever.MessageType, MessageAssembly
        
        public string SerialisedMessage { get; set; }
        
        public string SerialiserId { get; set; }

        public override void Validate()
        {
            
        }
    }
}
