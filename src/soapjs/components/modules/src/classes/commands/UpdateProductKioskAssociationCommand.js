import { validateArgs, types, ApiCommand } from '../../soap';

export class UpdateProductKioskAssociationCommand_c108v1 extends ApiCommand {
  constructor({ c108_productId, c108_applicableKioskIds }) {
    validateArgs(
      [{ c108_productId }, types.string],
      [{ c108_applicableKioskIds }, [types.string]],
    );

    super();

    this.schema = 'UpdateProductKioskAssociationCommand_c108v1';

    this.c108_productId = c108_productId;
    this.c108_applicableKioskIds = c108_applicableKioskIds;
  }
}
