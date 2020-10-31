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

function ownKeys(object, enumerableOnly) { var keys = Object.keys(object); if (Object.getOwnPropertySymbols) { var symbols = Object.getOwnPropertySymbols(object); if (enumerableOnly) symbols = symbols.filter(function (sym) { return Object.getOwnPropertyDescriptor(object, sym).enumerable; }); keys.push.apply(keys, symbols); } return keys; }

function _objectSpread(target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i] != null ? arguments[i] : {}; if (i % 2) { ownKeys(Object(source), true).forEach(function (key) { _defineProperty(target, key, source[key]); }); } else if (Object.getOwnPropertyDescriptors) { Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)); } else { ownKeys(Object(source)).forEach(function (key) { Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key)); }); } } return target; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var getSelectedOptions = function getSelectedOptions(selectedValues, options) {
  return options.filter(function (option) {
    return selectedValues.includes(option.value);
  });
};

var MultipleSelectDropdown = function MultipleSelectDropdown(props) {
  var reactSelectCustomStyling = {
    control: function control(base) {
      return _objectSpread(_objectSpread({}, base), {}, {
        boxShadow: 'none',
        '&:focus': {
          borderColor: 'hsl(0,0%,80%)'
        },
        '&:hover': {
          borderColor: 'hsl(0,0%,80%)'
        },
        borderColor: 'hsl(0,0%,80%)'
      });
    }
  };

  var handleChange = function handleChange(options) {
    var currentSelectedOptions = options ? options.map(function (option) {
      return option.value;
    }) : [];
    props.onChange(currentSelectedOptions);
  };

  return /*#__PURE__*/_react["default"].createElement(S.Select, {
    // formik
    name: props.name // react-select
    ,
    value: getSelectedOptions(props.value, props.options),
    onBlur: props.onBlur,
    onChange: handleChange,
    options: props.options,
    styles: reactSelectCustomStyling,
    placeholder: props.placeholder // styled-components
    ,
    width: props.width,
    "float": props["float"],
    minWidth: props.minWidth,
    isDisabled: props.disabled,
    isMulti: true
  });
};

MultipleSelectDropdown.propTypes = {
  name: _propTypes["default"].string,
  value: _propTypes["default"].arrayOf(_propTypes["default"].oneOfType([_propTypes["default"].number, _propTypes["default"].string])),
  onChange: _propTypes["default"].func,
  onBlur: _propTypes["default"].func,
  options: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    value: _propTypes["default"].oneOfType([_propTypes["default"].number, _propTypes["default"].string]).isRequired,
    label: _propTypes["default"].string.isRequired
  })).isRequired,
  width: _propTypes["default"].string,
  "float": _propTypes["default"].string,
  minWidth: _propTypes["default"].string,
  placeholder: _propTypes["default"].string,
  disabled: _propTypes["default"].bool
};
MultipleSelectDropdown.defaultProps = {
  width: '100%',
  "float": 'none',
  minWidth: '0',
  disabled: false
};
var _default = MultipleSelectDropdown;
exports["default"] = _default;