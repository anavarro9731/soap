namespace Soap.If.MessagePipeline.ProcessesAndOperations
{
    using System.Dynamic;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Utility.Objects.Binary;

    /// <summary>
    ///     stores the data for a process instance
    /// </summary>
    public class ProcessState : Aggregate
    {
        public Flags Flags { get; set; }

        public ExpandoObject References { get; set; } = new ExpandoObject();
    }
}