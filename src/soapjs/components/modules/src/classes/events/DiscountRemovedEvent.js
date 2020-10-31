import { validateArgs, types, ApiEvent } from '../../soap';

export class DiscountRemovedEvent_e131v1 extends ApiEvent {
  constructor({ e131_discountId }) {
    validateArgs([e131_discountId, types.string]);

    super();

    this.schema = 'DiscountRemovedEvent_e131v1';

    this.e131_discountId = e131_discountId;
  }
}
