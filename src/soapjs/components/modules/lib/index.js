"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
Object.defineProperty(exports, "ValueLabel", {
  enumerable: true,
  get: function get() {
    return _ValueLabel.ValueLabel;
  }
});
Object.defineProperty(exports, "useQuery", {
  enumerable: true,
  get: function get() {
    return _apiRequest.useQuery;
  }
});
Object.defineProperty(exports, "command", {
  enumerable: true,
  get: function get() {
    return _apiRequest.command;
  }
});
exports.utils = exports.style = exports.soap = exports.i18n = void 0;

var _ValueLabel = require("./modules/classes/shared-value-types/ValueLabel");

var _apiRequest = require("./modules/hooks/api-request");

var _i18n = _interopRequireWildcard(require("./modules/i18n/index"));

exports.i18n = _i18n;

var _soap = _interopRequireWildcard(require("./modules/soap/index"));

exports.soap = _soap;

var _style = _interopRequireWildcard(require("./modules/style/index"));

exports.style = _style;

var _utils = _interopRequireWildcard(require("./modules/utils/index"));

exports.utils = _utils;

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }