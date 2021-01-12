namespace Soap.Api.Sample.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Api.Sample.Models.ValueTypes;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class TestDataOperations : Operations<TestData>
    {
        public Func<C107v1_CreateOrUpdateTestDataTypes, Task> SetTestData =>
            async newState =>
                {
                {
                    var change = await DetermineChanges();

                    await change();
                }

                async Task<Func<Task>> DetermineChanges()
                {
                    if (await DataReader.ReadById<TestData>(newState.C107_Guid.Value) == null)
                    {
                        return async () => await DataWriter.Create(
                                               new TestData
                                               {
                                                   id = newState.C107_Guid.Value,
                                                   Boolean = newState.C107_Boolean,
                                                   BooleanOptional = newState.C107_BooleanOptional,
                                                   Decimal = newState.C107_Decimal,
                                                   DecimalOptional = newState.C107_DecimalOptional,
                                                   Guid = newState.C107_Guid,
                                                   GuidOptional = newState.C107_GuidOptional,
                                                   Long = newState.C107_LongOptional,
                                                   LongOptional = newState.C107_LongOptional,
                                                   DateTime = newState.C107_DateTime,
                                                   DateTimeOptional = newState.C107_DateTimeOptional,
                                                   CustomObject = new Address
                                                   {
                                                       House = newState.C107_CustomObject.C107_House,
                                                       Town = newState.C107_CustomObject.C107_Town,
                                                       PostCode = newState.C107_CustomObject.C107_PostCode
                                                   },
                                                   Image = newState.C107_Image,
                                                   ImageOptional = newState.C107_ImageOptional,
                                                   File = newState.C107_File,
                                                   FileOptional = newState.C107_FileOptional,
                                                   StringOptionalMultiline = newState.C107_StringOptionalMultiline,
                                                   StringOptional = newState.C107_StringOptional,
                                                   String = newState.C107_String,
                                                   PostCodesMulti = newState.C107_PostCodesMulti,
                                                   PostCodesSingle = newState.C107_PostCodesSingle,
                                                   PostCodesMultiOptional = newState.C107_PostCodesMultiOptional,
                                                   PostCodesSingleOptional = newState.C107_PostCodesSingleOptional
                                               });
                    }

                    return async () => await DataWriter.UpdateById<TestData>(
                                           newState.C107_Guid.Value,
                                           data =>
                                               {
                                               data.id = newState.C107_Guid.Value;
                                               data.Boolean = newState.C107_Boolean;
                                               data.BooleanOptional = newState.C107_BooleanOptional;
                                               data.Decimal = newState.C107_Decimal;
                                               data.DecimalOptional = newState.C107_DecimalOptional;
                                               data.Guid = newState.C107_Guid;
                                               data.GuidOptional = newState.C107_GuidOptional;
                                               data.Long = newState.C107_LongOptional;
                                               data.LongOptional = newState.C107_LongOptional;
                                               data.DateTime = newState.C107_DateTime;
                                               data.DateTimeOptional = newState.C107_DateTimeOptional;
                                               data.CustomObject = new Address
                                               {
                                                   House = newState.C107_CustomObject.C107_House,
                                                   Town = newState.C107_CustomObject.C107_Town,
                                                   PostCode = newState.C107_CustomObject.C107_PostCode
                                               };
                                               data.Image = newState.C107_Image;
                                               data.ImageOptional = newState.C107_ImageOptional;
                                               data.File = newState.C107_File;
                                               data.FileOptional = newState.C107_FileOptional;
                                               data.StringOptionalMultiline = newState.C107_StringOptionalMultiline;
                                               data.StringOptional = newState.C107_StringOptional;
                                               data.String = newState.C107_String;
                                               data.PostCodesMulti = newState.C107_PostCodesMulti;
                                               data.PostCodesSingle = newState.C107_PostCodesSingle;
                                               data.PostCodesMultiOptional = newState.C107_PostCodesMultiOptional;
                                               data.PostCodesSingleOptional = newState.C107_PostCodesSingleOptional;
                                               });
                }
                };
    }
}
