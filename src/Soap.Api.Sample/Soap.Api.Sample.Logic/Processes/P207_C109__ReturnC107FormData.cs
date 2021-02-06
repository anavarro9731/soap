namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Mainwave.MimeTypes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.PfBase.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class P207_C109__ReturnC107FormData : Process, IBeginProcess<C109v1_GetC107FormData>
    {
        /* there is always a security concern to consider when writing processes that return form data
        if the form data is not default data, and that is whether the user has access to that data.
        this is why there should be separate commands for retrieving the default state of a form and
        for a populated state. you could share the same response event and just populate it differntly
        that is ok. there is also the matter of whether the user has permissions to send the subsequent
        command posed by the form presented but that is handled through the normal security process which 
        could result in the scenario where you could request a form with default values that you do not
        have the right to send. unless the default values are sensitive data it shouldnt be an issue */

        private readonly List<Enumeration> postCodes = new List<Enumeration>
        {
            new Enumeration("hr20dn", "HR2 0DN"),
            new Enumeration("al55ng", "AL55NG"),
            new Enumeration("ox29ju", "OX2 9JU")
        };

        
        public Func<C109v1_GetC107FormData, Task> BeginProcess =>
            async message =>
                {
                {
                    var c107PostCodesMultiOptional = new EnumerationAndFlags(allEnumerations: this.postCodes);
                    c107PostCodesMultiOptional.AddFlag(this.postCodes.First());
                    c107PostCodesMultiOptional.AddFlag(this.postCodes.Last());
            
            
                    var formData = new C107v1_CreateOrUpdateTestDataTypes
                    {
                        C107_Decimal = 0,
                        C107_Guid = Guid.NewGuid(),
                        C107_DateTime = DateTime.UtcNow,
                        C107_Long = 0,
                        C107_String = "test default",
                        C107_PostCodesSingle = new EnumerationAndFlags(this.postCodes.First(), this.postCodes, false),
                        C107_PostCodesSingleOptional = new EnumerationAndFlags(this.postCodes.Last(), this.postCodes, false),
                        C107_PostCodesMulti = new EnumerationAndFlags(this.postCodes.First(), this.postCodes),
                        C107_PostCodesMultiOptional = c107PostCodesMultiOptional,
                        C107_CustomObject = new C107v1_CreateOrUpdateTestDataTypes.Address
                        {
                            C107_Town = "Pontrilas"
                        },
                        C107_File = SampleBlobs.File1,
                        C107_Image = SampleBlobs.Image1
                    };
                    
                    if (BlobsNeedToBeSaved())
                    {
                        await SaveTestBlobs();
                    }

                    await PublishFormDataEvent(Bus, new E103v1_GotC107FormData(), formData);
                }

                static bool BlobsNeedToBeSaved() =>
                    ContextWithMessageLogEntry.Current.AppConfig.Environment != SoapEnvironments.InMemory;

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
