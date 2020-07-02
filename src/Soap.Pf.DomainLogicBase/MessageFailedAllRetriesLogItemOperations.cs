// TODO
// namespace Soap.Pf.DomainLogicBase
// {
//     using System;
//     using System.Threading.Tasks;
//     using Soap.If.MessagePipeline.ProcessesAndOperations;
//     using Soap.MessagePipeline.ProcessesAndOperations;
//     using Soap.Pf.DomainModelsBase;
//
//     public class MessageFailedAllRetriesLogItemOperations : Operations<MessageFailedAllRetriesLogItem>
//     {
//         public Task<MessageFailedAllRetriesLogItem> AddLogItem(Guid messageId)
//         {
//             {
//                 DetermineChange(out var logItem);
//
//                 return DataStore.Create(logItem);
//             }
//
//             void DetermineChange(out MessageFailedAllRetriesLogItem logItem)
//             {
//                 logItem = new MessageFailedAllRetriesLogItem
//                 {
//                     IdOfMessageThatFailed = messageId
//                 };
//             }
//         }
//     }
// }