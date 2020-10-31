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
