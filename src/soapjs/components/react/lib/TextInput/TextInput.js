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

var TextInput = function TextInput(props) {
  return /*#__PURE__*/_react["default"].createElement(S.TextInput, {
    value: props.value,
    onChange: function onChange(e) {
      return props.onChange(e.target.value);
    },
    onBlur: props.onBlur,
    type: props.type,
    disabled: props.disabled,
    autoComplete: "off" // formik
    ,
    name: props.name // styled-components
    ,
    width: props.width,
    border: props.border
  });
};

TextInput.propTypes = {
  name: _propTypes["default"].string,
  value: _propTypes["default"].oneOfType([_propTypes["default"].number, _propTypes["default"].string]),
  onChange: _propTypes["default"].func,
  onBlur: _propTypes["default"].func,
  width: _propTypes["default"].string,
  disabled: _propTypes["default"].bool,
  type: _propTypes["default"].string,
  border: _propTypes["default"].string
};
TextInput.defaultProps = {
  width: '100%',
  disabled: false,
  type: 'text',
  border: "1px solid ".concat(_defaults.defaultBorderColour)
};
var _default = TextInput;
exports["default"] = _default;