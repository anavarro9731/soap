"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.HideChildren = exports.LoadingIcon = void 0;

var _styledComponents = _interopRequireWildcard(require("styled-components"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  // contents means it behaves like the div doesn't exist\n  display: ", ";\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  height: 100%;\n  width: 100%;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n\n  :after {\n    color: #fdc798;\n    font-size: 80px;\n    content: ' .';\n    font-family: 'Comic Sans MS';\n    animation: ", " 1s steps(5, end) infinite;\n  }\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n      0%, 20% {\n        color: #fdc798;\n        text-shadow:\n          .25em 0 0 #fdc798,\n          .5em 0 0 #fdc798;\n        }\n      40% {\n        color: #ff7d09;\n        text-shadow:\n          .25em 0 0 #fdc798,\n          .5em 0 0 #fdc798;\n        }\n      60% {\n        text-shadow:\n          .25em 0 0 #ff7d09,\n          .5em 0 0 #fdc798;\n        }\n      80%, 100% {\n        text-shadow:\n          .25em 0 0 #ff7d09,\n          .5em 0 0 #ff7d09;\n        }\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var dots = (0, _styledComponents.keyframes)(_templateObject());

var LoadingIcon = _styledComponents["default"].div(_templateObject2(), dots);

exports.LoadingIcon = LoadingIcon;

var HideChildren = _styledComponents["default"].div(_templateObject3(), function (props) {
  return props.hide ? 'none' : 'contents';
});

exports.HideChildren = HideChildren;