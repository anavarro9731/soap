import { validateArgs, types, ApiEvent } from '../../soap';

export class ApiErrorOccurred extends ApiEvent {
  constructor({ message }) {
    validateArgs([message, types.string]);

    super();

    this.schema = 'ApiErrorOccurred';

    this.message = message;
  }
}
