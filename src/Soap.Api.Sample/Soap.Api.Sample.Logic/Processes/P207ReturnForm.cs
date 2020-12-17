namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Dynamic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore.Models.PureFunctions.Extensions;
    using Mainwave.MimeTypes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Commands.UI;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Pf.MessageContractsBase;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Operations;

    public class P207ReturnForm : Process, IBeginProcess<C109v1_GetForm>
    {
        public Func<C109v1_GetForm, Task> BeginProcess =>
            async message =>
                {
                var eventName =
                    $"{typeof(E103v1_GetC107Form).Namespace}.{message.C109_FormDataEventName}, {typeof(E103v1_GetC107Form).Assembly.GetName().Name}";

                var formDataEventType = Type.GetType(eventName);

                Guard.Against(formDataEventType == null, "Cannot find command of type: " + message.C109_FormDataEventName);
                Guard.Against(
                    !formDataEventType.InheritsOrImplements(typeof(UIFormDataEvent)),
                    $"Specified command {message.C109_FormDataEventName} does not inherit from {nameof(UIFormDataEvent)}");

                var blobStorage = ContextWithMessageLogEntry.Current.BlobStorage;

                var imageBytes = Resources.ExtractResource("soap.jpg", SoapApiSampleLogic.GetAssembly);
                var image = new BlobMeta
                {
                    Id = Guid.Parse("457140C3-933D-4B5A-9D1F-0BDCA8D3FAA9"),
                    Name = "soap.jpg"
                };

                if (!await blobStorage.Exists(image.Id.Value))
                {
                    await blobStorage.SaveByteArrayAsBlob(imageBytes, image.Id.Value, MimeType.Image.Jpeg);
                }

                var fileBytes = Resources.ExtractResource("soap.zip", SoapApiSampleLogic.GetAssembly);
                var file = new BlobMeta
                {
                    Id = Guid.Parse("5858C003-CCA0-48CE-B073-381A57A8AB61"),
                    Name = "soap.zip"
                };
                if (!await blobStorage.Exists(file.Id.Value))
                {
                    await blobStorage.SaveByteArrayAsBlob(fileBytes, file.Id.Value, MimeType.Application.Zip);
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
                              dynamic x = new ExpandoObject();
                              x.Image = image;
                              x.File = file;
                              e.SetProperties(sasToken, commandId, x);
                              });

                await Publish(@event);
                };
    }
}
