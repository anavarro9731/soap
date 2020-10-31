"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _keys = _interopRequireDefault(require("../keys"));

var _translation;

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var _default = {
  // translation is the i18n default namespace
  translation: (_translation = {}, _defineProperty(_translation, _keys["default"].add, 'Add'), _defineProperty(_translation, _keys["default"].back, 'Back'), _defineProperty(_translation, _keys["default"].no, 'No'), _defineProperty(_translation, _keys["default"].optional, 'Optional'), _defineProperty(_translation, _keys["default"].required, 'Required'), _defineProperty(_translation, _keys["default"].save, 'Save'), _defineProperty(_translation, _keys["default"].yes, 'Yes'), _translation)
};
exports["default"] = _default;