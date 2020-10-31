import { validateArgs, types, ApiEvent } from '../../soap';

export class GetAllergensByProductIdQueryReadyEvent_e101v1 extends ApiEvent {
  constructor({
    e101_celery,
    e101_cereal,
    e101_crustaceans,
    e101_eggs,
    e101_fish,
    e101_lupin,
    e101_milk,
    e101_molluscs,
    e101_mustard,
    e101_nuts,
    e101_peanut,
    e101_seasame,
    e101_soya,
    e101_sulphar,
  }) {
    validateArgs(
      [{ e101_celery }, types.boolean],
      [{ e101_cereal }, types.boolean],
      [{ e101_crustaceans }, types.boolean],
      [{ e101_eggs }, types.boolean],
      [{ e101_fish }, types.boolean],
      [{ e101_lupin }, types.boolean],
      [{ e101_milk }, types.boolean],
      [{ e101_molluscs }, types.boolean],
      [{ e101_mustard }, types.boolean],
      [{ e101_nuts }, types.boolean],
      [{ e101_peanut }, types.boolean],
      [{ e101_seasame }, types.boolean],
      [{ e101_soya }, types.boolean],
      [{ e101_sulphar }, types.boolean],
    );

    super();

    this.schema = 'GetAllergensByProductIdQueryReadyEvent_e101v1';

    this.e101_celery = e101_celery;
    this.e101_cereal = e101_cereal;
    this.e101_crustaceans = e101_crustaceans;
    this.e101_eggs = e101_eggs;
    this.e101_fish = e101_fish;
    this.e101_lupin = e101_lupin;
    this.e101_milk = e101_milk;
    this.e101_molluscs = e101_molluscs;
    this.e101_mustard = e101_mustard;
    this.e101_nuts = e101_nuts;
    this.e101_peanut = e101_peanut;
    this.e101_seasame = e101_seasame;
    this.e101_soya = e101_soya;
    this.e101_sulphar = e101_sulphar;
  }
}
