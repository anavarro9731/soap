import { validateArgs, types, ApiCommand } from '../../soap';

export class RemoveDiscountCommand_c102v1 extends ApiCommand {
  constructor({ c102_discountId }) {
    validateArgs([{ c102_discountId }, types.string]);

    super();

    this.schema = 'RemoveDiscountCommand_c102v1';

    this.c102_discountId = c102_discountId;
  }
}
