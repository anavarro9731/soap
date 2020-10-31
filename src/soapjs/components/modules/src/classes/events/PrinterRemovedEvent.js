import { validateArgs, types, ApiEvent } from '../../soap';

export class PrinterRemovedEvent_e130v1 extends ApiEvent {
  constructor({ e130_printerId, e130_siteId }) {
    validateArgs([e130_printerId, types.string], [e130_siteId, types.string]);

    super();

    this.schema = 'PrinterRemovedEvent_e130v1';

    this.e130_printerId = e130_printerId;
    this.e130_siteId = e130_siteId;
  }
}
