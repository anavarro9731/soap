"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _TextInput = _interopRequireDefault(require("../TextInput"));

var S = _interopRequireWildcard(require("./style"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

var TextInputWithSymbol = function TextInputWithSymbol(props) {
  var alignStylingProp = {
    rightAlignSymbol: props.rightAlignSymbol,
    symbolColour: props.symbolColour
  };

  var symbol = /*#__PURE__*/_react["default"].createElement(S.Symbol, alignStylingProp, props.symbol);

  return /*#__PURE__*/_react["default"].createElement(S.Grid, alignStylingProp, !props.rightAlignSymbol && symbol, /*#__PURE__*/_react["default"].createElement(_TextInput["default"], _extends({}, props, {
    type: props.type,
    border: "none"
  })), props.rightAlignSymbol && symbol);
};

TextInputWithSymbol.propTypes = {
  rightAlignSymbol: _propTypes["default"].bool,
  type: _propTypes["default"].string,
  symbol: _propTypes["default"].string.isRequired,
  symbolColour: _propTypes["default"].string
};
TextInputWithSymbol.defaultProps = {
  rightAlignSymbol: false,
  type: 'number',
  symbolColour: '#676767'
};
var _default = TextInputWithSymbol;
exports["default"] = _default;