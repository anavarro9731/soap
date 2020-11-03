import { translate, keys } from '../i18n';

export const displayYesOrNo = boolean =>
  boolean ? translate(keys.yes) : translate(keys.no);

export const convertNumberToCurrencyString = (number, currency = 'GBP', locales = 'en-GB') => {
  const currencyFormatter = new Intl.NumberFormat(locales, {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  return currencyFormatter.format(number);
};

export const convertToRoundedNumber = (string, decimalPlaces = 2) =>
  parseFloat(parseFloat(string).toFixed(decimalPlaces)) || 0;

export const sortArrayByObjectPropertyAlphanumerically = (
    array,
    getObjectProperty,
) =>
    array.sort((first, second) => {
      const firstValue = getObjectProperty(first);
      const secondValue = getObjectProperty(second);
      if (firstValue && secondValue) {
        return firstValue.localeCompare(secondValue, undefined, {
          numeric: true,
          sensitivity: 'base',
        });
      }
      return -1;
    });
