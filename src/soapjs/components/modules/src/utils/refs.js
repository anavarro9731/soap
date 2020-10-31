export const getSelectRefOptions = selectRef =>
  Array.prototype.map.call(selectRef.current.options, option => ({
    value: option.value,
    label: option.label,
  }));
