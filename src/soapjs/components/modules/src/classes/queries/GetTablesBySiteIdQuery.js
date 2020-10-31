import { validateArgs, types, ApiQuery } from '../../soap';

export class GetTablesBySiteIdQuery_q125v1 extends ApiQuery {
  constructor({ q125_siteId }) {
    validateArgs([{ q125_siteId }, types.string]);

    super();

    this.schema = 'GetTablesBySiteIdQuery_q125v1';

    this.q125_siteId = q125_siteId;
  }
}
