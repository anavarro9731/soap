import { validateArgs, types, ApiCommand, optional } from '../../soap';

export class CreateDiscountCommand_c101v1 extends ApiCommand {
  constructor({
    c101_discountId,
    c101_discountName,
    c101_discountType,
    c101_discountAmount,
    c101_daysDiscountIsActive,
    c101_minimumSpend,
    c101_discountCode,
    c101_includeModifiers,
    c101_onSaleNow,
    c101_freeModifiers,
    c101_startTime,
    c101_endTime,
    c101_allDay,
  }) {
    validateArgs(
      [{ c101_discountId }, types.string],
      [{ c101_discountName }, types.string],
      [{ c101_discountType }, types.string],
      [{ c101_discountAmount }, types.number],
      [{ c101_daysDiscountIsActive }, [types.string]],
      [{ c101_minimumSpend }, types.number],
      [{ c101_discountCode }, types.string],
      [{ c101_includeModifiers }, types.boolean],
      [{ c101_onSaleNow }, types.boolean],
      [{ c101_freeModifiers }, types.boolean],
      [{ c101_startTime }, types.string, optional],
      [{ c101_endTime }, types.string, optional],
      [{ c101_allDay }, types.boolean],
    );

    super();

    this.schema = 'CreateDiscountCommand_c101v1';

    this.c101_discountId = c101_discountId;
    this.c101_discountName = c101_discountName;
    this.c101_discountType = c101_discountType;
    this.c101_discountAmount = c101_discountAmount;
    this.c101_daysDiscountIsActive = c101_daysDiscountIsActive;
    this.c101_minimumSpend = c101_minimumSpend;
    this.c101_discountCode = c101_discountCode;
    this.c101_includeModifiers = c101_includeModifiers;
    this.c101_onSaleNow = c101_onSaleNow;
    this.c101_freeModifiers = c101_freeModifiers;
    this.c101_startTime = c101_startTime;
    this.c101_endTime = c101_endTime;
    this.c101_allDay = c101_allDay;
  }
}
