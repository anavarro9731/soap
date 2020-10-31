"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _defaults = require("../../modules/style/defaults");

var S = _interopRequireWildcard(require("./style"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var Button = function Button(props) {
  return /*#__PURE__*/_react["default"].createElement(S.Button, {
    colour: props.textColour,
    backgroundColour: props.backgroundColour,
    verticalPadding: props.verticalPadding,
    horizontalPadding: props.horizontalPadding,
    onClick: props.onClick,
    border: props.border,
    noLeftBorder: props.noLeftBorder,
    borderRadiusLeft: props.borderRadiusLeft,
    borderRadiusRight: props.borderRadiusRight,
    width: props.width,
    height: props.height,
    hoverBackgroundColour: props.hoverBackgroundColour,
    disabled: props.disabled,
    disabledBackgroundColour: props.disabledBackgroundColour,
    type: props.type,
    fontSize: props.fontSize
  }, props.children);
};

Button.propTypes = {
  children: _propTypes["default"].string,
  textColour: _propTypes["default"].string,
  backgroundColour: _propTypes["default"].string,
  onClick: _propTypes["default"].func,
  verticalPadding: _propTypes["default"].string,
  horizontalPadding: _propTypes["default"].string,
  border: _propTypes["default"].string,
  borderRadiusLeft: _propTypes["default"].string,
  borderRadiusRight: _propTypes["default"].string,
  hoverBackgroundColour: _propTypes["default"].string,
  type: _propTypes["default"].string,
  disabled: _propTypes["default"].bool,
  disabledBackgroundColour: _propTypes["default"].string,
  noLeftBorder: _propTypes["default"].bool,
  fontSize: _propTypes["default"].string
};
Button.defaultProps = {
  textColour: _defaults.defaultLightTextColour,
  backgroundColour: _defaults.defaultPrimaryBackgroundColour,
  verticalPadding: '15px',
  horizontalPadding: '25px',
  border: 'none',
  borderRadiusLeft: '5px',
  borderRadiusRight: '5px',
  type: 'button',
  width: 'auto',
  height: 'auto',
  disabled: false,
  disabledBackgroundColour: _defaults.defaultDisabledColour,
  noLeftBorder: false,
  fontSize: '16px'
};
var _default = Button;
exports["default"] = _default;