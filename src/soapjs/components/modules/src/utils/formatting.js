import { translate, keys } from '../i18n';

export const displayYesOrNo = boolean =>
  boolean ? translate(keys.yes) : translate(keys.no);

export const convertNumberToCurrencyString = (number, currency = 'GBP') => {
  const currencyFormatter = new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  return currencyFormatter.format(number);
};

export const convertToRoundedNumber = (string, decimalPlaces = 2) =>
  parseFloat(parseFloat(string).toFixed(decimalPlaces)) || 0;
