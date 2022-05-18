namespace Soap.Client
{
    using System;

    public struct StatefulProcessId
    {
        public StatefulProcessId(string typeId, Guid instanceId)
        {
            TypeId = typeId;
            InstanceId = instanceId;
        }

        public string TypeId { get; set; }

        public Guid InstanceId { get; set; }

        public override string ToString() => $"{TypeId}:{InstanceId}";
    }
}