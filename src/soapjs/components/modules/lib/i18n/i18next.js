"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = exports.useNewLanguage = void 0;

var _i18next = _interopRequireDefault(require("i18next"));

var _en = _interopRequireDefault(require("./translations/en"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

_i18next["default"].init({
  interpolation: {
    // React already does escaping
    escapeValue: false
  },
  lng: 'en',
  fallbackLng: 'en',
  resources: {
    en: _en["default"]
  }
});

var useNewLanguage = function useNewLanguage(language, translations) {
  var validLanguageAndTranslations = language && translations && typeof language === 'string' && _typeof(translations) === 'object';

  if (validLanguageAndTranslations) {
    _i18next["default"].addResourceBundle(language, 'translation', translations);

    _i18next["default"].changeLanguage(language);
  }
};

exports.useNewLanguage = useNewLanguage;
var _default = _i18next["default"];
exports["default"] = _default;