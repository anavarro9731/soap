import { translate, keys } from '../i18n';

export const daysOfTheWeekSelectionOptions = [
  { value: keys.monday, label: translate(keys.monday) },
  { value: keys.tuesday, label: translate(keys.tuesday) },
  { value: keys.thursday, label: translate(keys.wednesday) },
  { value: keys.thursday, label: translate(keys.thursday) },
  { value: keys.friday, label: translate(keys.friday) },
  { value: keys.saturday, label: translate(keys.saturday) },
  { value: keys.sunday, label: translate(keys.sunday) },
];

export const timeSelectionOptions = (interval = 1) => {
  if (60 % interval !== 0)
    throw "timeSelectionOptions Function: The 'interval' parameter must be a number that 60 is divisible by";
  return Array.from(
    {
      length: (24 * 60) / interval,
    },
    (_, index) => {
      const hours = ('0' + Math.floor((index * interval) / 60)).slice(-2);
      const minutes = ('0' + (index % (60 / interval)) * interval).slice(-2);

      const time = `${hours}:${minutes}`;

    //  const timeWithSeconds = `${hours}:${minutes}:00`;

      return { value: time, label: time };
    },
  );
};

export const getUnselectedDropdownOptions = (
  allOptions,
  alreadySelectedOptionValues,
) => currentDropdownValue => {
  const currentDropdownOption =
    allOptions &&
    allOptions.find(option => option.value === currentDropdownValue);

  const unselectedOptions = allOptions.filter(
    option => !alreadySelectedOptionValues.includes(option.value),
  );
  if (!currentDropdownOption) return unselectedOptions;

  return [...unselectedOptions, currentDropdownOption];
};
