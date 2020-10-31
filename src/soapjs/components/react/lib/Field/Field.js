"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _formik = require("formik");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

function _objectWithoutProperties(source, excluded) { if (source == null) return {}; var target = _objectWithoutPropertiesLoose(source, excluded); var key, i; if (Object.getOwnPropertySymbols) { var sourceSymbolKeys = Object.getOwnPropertySymbols(source); for (i = 0; i < sourceSymbolKeys.length; i++) { key = sourceSymbolKeys[i]; if (excluded.indexOf(key) >= 0) continue; if (!Object.prototype.propertyIsEnumerable.call(source, key)) continue; target[key] = source[key]; } } return target; }

function _objectWithoutPropertiesLoose(source, excluded) { if (source == null) return {}; var target = {}; var sourceKeys = Object.keys(source); var key, i; for (i = 0; i < sourceKeys.length; i++) { key = sourceKeys[i]; if (excluded.indexOf(key) >= 0) continue; target[key] = source[key]; } return target; }

var Field = function Field(props) {
  var Component = props.component,
      field = props.field,
      inputComponentSpecificProps = _objectWithoutProperties(props, ["component", "field"]);

  var _useFormikContext = (0, _formik.useFormikContext)(),
      setFieldValue = _useFormikContext.setFieldValue;

  return /*#__PURE__*/_react["default"].createElement(Component, _extends({
    name: field.name,
    value: field.value,
    onChange: function onChange(newValue) {
      return setFieldValue(field.name, newValue);
    },
    onBlur: field.onBlur
  }, inputComponentSpecificProps));
};

Field.propTypes = {
  field: _propTypes["default"].object.isRequired,
  component: _propTypes["default"].func.isRequired
};
var _default = Field;
exports["default"] = _default;