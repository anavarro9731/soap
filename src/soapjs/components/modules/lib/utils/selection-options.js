"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.getUnselectedDropdownOptions = exports.timeSelectionOptions = exports.daysOfTheWeekSelectionOptions = exports.daysOfTheWeek = void 0;

var _i18n = require("../i18n");

function _toConsumableArray(arr) { return _arrayWithoutHoles(arr) || _iterableToArray(arr) || _unsupportedIterableToArray(arr) || _nonIterableSpread(); }

function _nonIterableSpread() { throw new TypeError("Invalid attempt to spread non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _iterableToArray(iter) { if (typeof Symbol !== "undefined" && Symbol.iterator in Object(iter)) return Array.from(iter); }

function _arrayWithoutHoles(arr) { if (Array.isArray(arr)) return _arrayLikeToArray(arr); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

var daysOfTheWeek = {
  MONDAY: 'Monday',
  TUESDAY: 'Tuesday',
  WEDNESDAY: 'Wednesday',
  THURSDAY: 'Thursday',
  FRIDAY: 'Friday',
  SATURDAY: 'Saturday',
  SUNDAY: 'Sunday'
};
exports.daysOfTheWeek = daysOfTheWeek;
var daysOfTheWeekSelectionOptions = [{
  value: daysOfTheWeek.MONDAY,
  label: (0, _i18n.translate)(_i18n.keys.monday)
}, {
  value: daysOfTheWeek.TUESDAY,
  label: (0, _i18n.translate)(_i18n.keys.tuesday)
}, {
  value: daysOfTheWeek.WEDNESDAY,
  label: (0, _i18n.translate)(_i18n.keys.wednesday)
}, {
  value: daysOfTheWeek.THURSDAY,
  label: (0, _i18n.translate)(_i18n.keys.thursday)
}, {
  value: daysOfTheWeek.FRIDAY,
  label: (0, _i18n.translate)(_i18n.keys.friday)
}, {
  value: daysOfTheWeek.SATURDAY,
  label: (0, _i18n.translate)(_i18n.keys.saturday)
}, {
  value: daysOfTheWeek.SUNDAY,
  label: (0, _i18n.translate)(_i18n.keys.sunday)
}];
exports.daysOfTheWeekSelectionOptions = daysOfTheWeekSelectionOptions;

var timeSelectionOptions = function timeSelectionOptions() {
  var interval = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : 1;
  if (60 % interval !== 0) throw "timeSelectionOptions Function: The 'interval' parameter must be a number that 60 is divisible by";
  return Array.from({
    length: 24 * 60 / interval
  }, function (_, index) {
    var hours = ('0' + Math.floor(index * interval / 60)).slice(-2);
    var minutes = ('0' + index % (60 / interval) * interval).slice(-2);
    var time = "".concat(hours, ":").concat(minutes);
    var timeWithSeconds = "".concat(hours, ":").concat(minutes, ":00");
    return {
      value: timeWithSeconds,
      label: time
    };
  });
};

exports.timeSelectionOptions = timeSelectionOptions;

var getUnselectedDropdownOptions = function getUnselectedDropdownOptions(allOptions, alreadySelectedOptionValues) {
  return function (currentDropdownValue) {
    var currentDropdownOption = allOptions && allOptions.find(function (option) {
      return option.value === currentDropdownValue;
    });
    var unselectedOptions = allOptions.filter(function (option) {
      return !alreadySelectedOptionValues.includes(option.value);
    });
    if (!currentDropdownOption) return unselectedOptions;
    return [].concat(_toConsumableArray(unselectedOptions), [currentDropdownOption]);
  };
};

exports.getUnselectedDropdownOptions = getUnselectedDropdownOptions;