import { validateArgs, types } from './util';

export class ValueLabel {
  constructor({ label, value }) {
    validateArgs([{ label }, types.string], [{ value }, types.string]);

    this.label = label;
    this.value = value;
  }
}
