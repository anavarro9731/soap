"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
Object.defineProperty(exports, "keys", {
  enumerable: true,
  get: function get() {
    return _keys["default"];
  }
});
Object.defineProperty(exports, "translate", {
  enumerable: true,
  get: function get() {
    return _translate.translate;
  }
});
Object.defineProperty(exports, "useNewLanguage", {
  enumerable: true,
  get: function get() {
    return _i18next.useNewLanguage;
  }
});
Object.defineProperty(exports, "languages", {
  enumerable: true,
  get: function get() {
    return _languages["default"];
  }
});

var _keys = _interopRequireDefault(require("./keys"));

var _translate = require("./translate");

var _i18next = require("./i18next");

var _languages = _interopRequireDefault(require("./languages"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }