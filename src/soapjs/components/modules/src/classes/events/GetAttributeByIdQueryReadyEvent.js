import { validateArgs, types, ApiEvent } from '../../soap';

export class GetAttributeByIdQueryReadyEvent_e102v1 extends ApiEvent {
  constructor({
    e102_attributeId,
    e102_name,
    e102_dataTypeId,
    e102_isDefault,
    e102_isRequired,
  }) {
    validateArgs(
      [{ e102_attributeId }, types.string],
      [{ e102_name }, types.string],
      [{ e102_dataTypeId }, types.string],
      [{ e102_isDefault }, types.boolean],
      [{ e102_isRequired }, types.boolean],
    );

    super();

    this.schema = 'GetAttributeByIdQueryReadyEvent_e102v1';

    this.e102_attributeId = e102_attributeId;
    this.e102_name = e102_name;
    this.e102_dataTypeId = e102_dataTypeId;
    this.e102_isDefault = e102_isDefault;
    this.e102_isRequired = e102_isRequired;
  }
}
