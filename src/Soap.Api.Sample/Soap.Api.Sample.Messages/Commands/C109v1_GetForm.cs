namespace Soap.Api.Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;

    public class C109v1_GetForm : ApiCommand
    {
        public string? C109_FormDataEventName { get; set; }
    }
}
