"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.translate = void 0;

var _i18next = _interopRequireDefault(require("./i18next"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var translate = function translate(key) {
  return _i18next["default"].t(key);
};

exports.translate = translate;