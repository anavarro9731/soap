namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System.Dynamic;
    using DataStore.Interfaces.LowLevel;
    using Soap.Utility.Objects.Binary;

    /// <summary>
    ///     stores the data for a process instance
    /// </summary>
    public class ProcessState : Aggregate
    {
        public Flags Flags { get; set; } = new Flags();

        public ExpandoObject References { get; set; } = new ExpandoObject();
    }
}