import { validateArgs, types, ApiQuery } from '../../soap';

export class GetTimesByIntervalForSelectionQuery_q119v1 extends ApiQuery {
  constructor({ q119_interval }) {
    validateArgs([{ q119_interval }, types.number]);

    super();

    this.schema = 'GetTimesByIntervalForSelectionQuery_q119v1';

    this.q119_interval = q119_interval;
  }
}
