"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.sortArrayByObjectPropertyAlphanumerically = void 0;

var sortArrayByObjectPropertyAlphanumerically = function sortArrayByObjectPropertyAlphanumerically(array, getObjectProperty) {
  return array.sort(function (first, second) {
    var firstValue = getObjectProperty(first);
    var secondValue = getObjectProperty(second);

    if (firstValue && secondValue) {
      return firstValue.localeCompare(secondValue, undefined, {
        numeric: true,
        sensitivity: 'base'
      });
    }

    return -1;
  });
};

exports.sortArrayByObjectPropertyAlphanumerically = sortArrayByObjectPropertyAlphanumerically;