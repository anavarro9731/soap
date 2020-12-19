namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Mainwave.MimeTypes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.PfBase.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class P207ReturnForm : Process, IBeginProcess<C109v1_GetForm>
    {
        public Func<C109v1_GetForm, Task> BeginProcess =>
            async message =>
                {
                var eventName =
                    $"{typeof(E100v1_Pong).Namespace}.{message.C109_FormDataEventName}, {typeof(E100v1_Pong).Assembly.GetName().Name}";

                var formDataEventType = Type.GetType(eventName);

                Guard.Against(formDataEventType == null, "Cannot find command of type: " + message.C109_FormDataEventName);
                Guard.Against(
                    !formDataEventType.InheritsOrImplements(typeof(UIFormDataEvent)),
                    $"Specified command {message.C109_FormDataEventName} does not inherit from {nameof(UIFormDataEvent)}");

                if (message.C109_FormDataEventName == typeof(C107v1_CreateOrUpdateTestDataTypes).ToShortAssemblyTypeName())
                {
                    await SaveTestBlobs();
                }

                var @event = Activator.CreateInstance(formDataEventType) as ApiEvent;
                @event.As<UIFormDataEvent>()
                      .Op(
                          e =>
                              {
                              var commandId = Guid.NewGuid();
                              var sasToken = ContextWithMessageLogEntry.Current.BlobStorage.GetStorageSasTokenForBlob(
                                  commandId,
                                  new EnumerationFlags(IBlobStorage.BlobSasPermissions.CreateNew));
                              e.SetProperties(sasToken, commandId, ContextWithMessageLogEntry.Current.AppConfig.HttpApiEndpoint);
                              });

                await Publish(@event);
                };

        private async Task SaveTestBlobs()
        {
            var blobStorage = ContextWithMessageLogEntry.Current.BlobStorage;

            var imageBytes = Resources.ExtractResource("soap.jpg", SoapPfBaseMessages.GetAssembly);
            
            if (!await blobStorage.Exists(SampleBlobs.Image1.Id.Value))
            {
                await blobStorage.SaveByteArrayAsBlob(imageBytes, SampleBlobs.Image1.Id.Value, MimeType.Image.Jpeg);
            }

            var fileBytes = Resources.ExtractResource("soap.zip", SoapPfBaseMessages.GetAssembly);

            if (!await blobStorage.Exists(SampleBlobs.File1.Id.Value))
            {
                await blobStorage.SaveByteArrayAsBlob(fileBytes, SampleBlobs.File1.Id.Value, MimeType.Application.Zip);
            }
        }
    }
}
