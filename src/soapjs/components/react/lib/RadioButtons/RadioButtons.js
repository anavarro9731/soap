"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var S = _interopRequireWildcard(require("./style"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var RadioButtons = function RadioButtons(props) {
  var handleChange = function handleChange(event) {
    return props.onChange(event.target.value);
  };

  return /*#__PURE__*/_react["default"].createElement(S.Grid, {
    radioButtonsPerRow: props.radioButtonsPerRow
  }, props.options.map(function (option) {
    return /*#__PURE__*/_react["default"].createElement(S.MarginTopAndButton, {
      key: option.value
    }, /*#__PURE__*/_react["default"].createElement(S.RadioButton, {
      type: "radio",
      id: option.value,
      value: option.value,
      checked: option.value === props.value,
      onChange: handleChange,
      name: props.name,
      onBlur: props.onBlur,
      disabled: props.disabled
    }), /*#__PURE__*/_react["default"].createElement(S.Label, {
      htmlFor: option.value
    }, option.label));
  }));
};

RadioButtons.propTypes = {
  radioButtonsPerRow: _propTypes["default"].number,
  name: _propTypes["default"].string,
  value: _propTypes["default"].string,
  onBlur: _propTypes["default"].func,
  onChange: _propTypes["default"].func,
  options: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    value: _propTypes["default"].string.isRequired,
    label: _propTypes["default"].string.isRequired
  })).isRequired,
  disabled: _propTypes["default"].bool
};
RadioButtons.defaultProps = {
  disabled: false,
  radioButtonsPerRow: 3
};
var _default = RadioButtons;
exports["default"] = _default;