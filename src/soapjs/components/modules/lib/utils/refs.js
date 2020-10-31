"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.getSelectRefOptions = void 0;

var getSelectRefOptions = function getSelectRefOptions(selectRef) {
  return Array.prototype.map.call(selectRef.current.options, function (option) {
    return {
      value: option.value,
      label: option.label
    };
  });
};

exports.getSelectRefOptions = getSelectRefOptions;