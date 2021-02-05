namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Mainwave.MimeTypes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
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
        public class AuthErrorCodes : ErrorCode
        {
            /* Error Codes only need to be mapped if there is front-end logic that might depend on them
             otherwise the default error handling logic will do the job of returning the error message but without a specific code. */

            public static readonly ErrorCode NoApiPermissionExistsForThisMessage = Create(
                Guid.Parse("e21a4343-8daf-42fb-894f-0692c9299c21"),
                $"The user does not have permissions to execute the command requested by this command");

        }
        
        public Func<C109v1_GetForm, Task> BeginProcess =>
            async message =>
                {
                {
                    GetTypeInfo(message, out string eventShortAssemblyTypeName, out Type eventType, out string commandShortAssemblyTypeName, out Type commandType);

                    Guard.Against(eventType == null, "Cannot find command of type: " + message.C109_FormDataEventName);

                    Guard.Against(
                        !eventType.InheritsOrImplements(typeof(UIFormDataEvent)),
                        $"Specified command {message.C109_FormDataEventName} does not inherit from {nameof(UIFormDataEvent)}");

                    
                    /* because requesting form can in some cases mean returning data in the form that the user may not have the right to request
                    we need to test the users ability to send the command prepared by the event returned from this message. */
                    if (ContextWithMessageLogEntry.Current.AppConfig.AuthEnabled && 
                        !commandType.HasAttribute<AuthorisationNotRequired>())
                    {
                        Guard.Against(!Meta.IdentityPermissionsOrNull.ApiPermissions.Contains(commandType.Name),
                            AuthErrorCodes.NoApiPermissionExistsForThisMessage);    
                    }
                    
                    
                    if (BlobsNeedToBeSaved(eventShortAssemblyTypeName))
                    {
                        await SaveTestBlobs();
                    }

                    await PublishFormDataEvent(Bus, eventType);
                }

                static async Task PublishFormDataEvent(BusWrapper bus, Type formDataEventType)
                {
                    var @event = Activator.CreateInstance(formDataEventType) as ApiEvent;
                    @event.As<UIFormDataEvent>()
                          .Op(
                              e =>
                                  {
                                  var commandId = Guid.NewGuid();
                                  var sasToken = ContextWithMessageLogEntry.Current.BlobStorage.GetStorageSasTokenForBlob(
                                      commandId,
                                      new EnumerationFlags(IBlobStorage.BlobSasPermissions.CreateNew),
                                      "large-messages");
                                  e.SetProperties(sasToken, commandId);
                                  });

                    await bus.Publish(
                        @event,
                        new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
                }

                static void GetTypeInfo(C109v1_GetForm message, out string eventShortAssemblyTypeName, out Type eventType, out string commandShortAssemblyTypeName, out Type commandType)
                {
                    eventShortAssemblyTypeName = $"{message.C109_FormDataEventName}, {typeof(E100v1_Pong).Assembly.GetName().Name}";
                    eventType = Type.GetType(eventShortAssemblyTypeName);
                    var commandType1 = Activator.CreateInstance(eventType).As<UIFormDataEvent>().CommandType;
                    commandShortAssemblyTypeName = commandType1.ToShortAssemblyTypeName();
                    commandType = commandType1;
                }

                static bool BlobsNeedToBeSaved(string eventName) =>
                    eventName == typeof(E103v1_GetC107Form).ToShortAssemblyTypeName()
                    && ContextWithMessageLogEntry.Current.AppConfig.Environment != SoapEnvironments.InMemory;

                static async Task SaveTestBlobs()
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
                };
    }
}
