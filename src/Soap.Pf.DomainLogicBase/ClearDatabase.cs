namespace Soap.Pf.DomainLogicBase
{
    using System;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;

    public static class ClearDatabase
    {
        public static async Task ExecuteOutsideTransaction(
            IDocumentRepository documentRepository,
            IDataStoreQueryCapabilities readOperations,
            Guid? envelopeId,
            ApiMessageMeta currentMessageMeta)
        {
            MessageLogItem envelopeMessage = null;

            if (envelopeId.HasValue)
            {
                envelopeMessage = await readOperations.ReadActiveById<MessageLogItem>(envelopeId.Value);
            }

            await ((IResetData)documentRepository).NonTransactionalReset();

            if (envelopeMessage != null)
            {
                await documentRepository.AddAsync(
                    new ReplaceMessageLogItemOperation
                    {
                        Model = envelopeMessage
                    });
            }

            await documentRepository.AddAsync(
                new ReplaceMessageLogItemOperation
                {
                    Model = currentMessageMeta.MessageLogItem
                });
        }

        public class ReplaceMessageLogItemOperation : IDataStoreWriteOperation<MessageLogItem>
        {
            public DateTime Created { get; set; }

            public string MethodCalled { get; set; }

            public MessageLogItem Model { get; set; }

            public double StateOperationCost { get; set; }

            public TimeSpan? StateOperationDuration { get; set; }

            public long StateOperationStartTimestamp { get; set; }

            public long? StateOperationStopTimestamp { get; set; }

            public string TypeName { get; set; }
        }
    }
}