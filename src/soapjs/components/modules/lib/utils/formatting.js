"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.convertToRoundedNumber = exports.convertNumberToCurrencyString = exports.displayYesOrNo = void 0;

var _i18n = require("../i18n");

var displayYesOrNo = function displayYesOrNo(_boolean) {
  return _boolean ? (0, _i18n.translate)(_i18n.keys.yes) : (0, _i18n.translate)(_i18n.keys.no);
};

exports.displayYesOrNo = displayYesOrNo;

var convertNumberToCurrencyString = function convertNumberToCurrencyString(number) {
  var currency = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : 'GBP';
  var currencyFormatter = new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
  return currencyFormatter.format(number);
};

exports.convertNumberToCurrencyString = convertNumberToCurrencyString;

var convertToRoundedNumber = function convertToRoundedNumber(string) {
  var decimalPlaces = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : 2;
  return parseFloat(parseFloat(string).toFixed(decimalPlaces)) || 0;
};

exports.convertToRoundedNumber = convertToRoundedNumber;