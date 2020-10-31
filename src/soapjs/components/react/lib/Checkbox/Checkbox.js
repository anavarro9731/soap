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

var Checkbox = function Checkbox(props) {
  var checkboxId = "checkbox-component-".concat(props.name || Math.random());

  var label = /*#__PURE__*/_react["default"].createElement(S.Label, {
    htmlFor: checkboxId,
    disabled: props.disabled,
    labelOnLeftSide: props.labelOnLeftSide
  }, props.checkboxLabel);

  return /*#__PURE__*/_react["default"].createElement(S.CheckboxWithLabel, null, props.labelOnLeftSide && label, /*#__PURE__*/_react["default"].createElement(S.Checkbox, {
    id: checkboxId,
    checked: props.value,
    onChange: function onChange(e) {
      return props.onChange(e.target.checked);
    },
    onBlur: props.onBlur,
    type: "checkbox",
    disabled: props.disabled // formik
    ,
    name: props.name
  }), !props.labelOnLeftSide && label);
};

Checkbox.propTypes = {
  name: _propTypes["default"].string,
  value: _propTypes["default"].bool,
  onChange: _propTypes["default"].func,
  onBlur: _propTypes["default"].func,
  disabled: _propTypes["default"].bool,
  checkboxLabel: _propTypes["default"].string,
  labelOnLeftSide: _propTypes["default"].bool
};
Checkbox.defaultProps = {
  disabled: false,
  value: false,
  labelOnLeftSide: false
};
var _default = Checkbox;
exports["default"] = _default;