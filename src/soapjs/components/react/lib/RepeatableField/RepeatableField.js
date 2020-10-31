"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _Grid = _interopRequireDefault(require("@material-ui/core/Grid"));

var _Button = _interopRequireDefault(require("../Button"));

var _i18n = require("../../modules/i18n");

var _formik = require("formik");

var S = _interopRequireWildcard(require("./style"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _iterableToArrayLimit(arr, i) { if (typeof Symbol === "undefined" || !(Symbol.iterator in Object(arr))) return; var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

function _objectWithoutProperties(source, excluded) { if (source == null) return {}; var target = _objectWithoutPropertiesLoose(source, excluded); var key, i; if (Object.getOwnPropertySymbols) { var sourceSymbolKeys = Object.getOwnPropertySymbols(source); for (i = 0; i < sourceSymbolKeys.length; i++) { key = sourceSymbolKeys[i]; if (excluded.indexOf(key) >= 0) continue; if (!Object.prototype.propertyIsEnumerable.call(source, key)) continue; target[key] = source[key]; } } return target; }

function _objectWithoutPropertiesLoose(source, excluded) { if (source == null) return {}; var target = {}; var sourceKeys = Object.keys(source); var key, i; for (i = 0; i < sourceKeys.length; i++) { key = sourceKeys[i]; if (excluded.indexOf(key) >= 0) continue; target[key] = source[key]; } return target; }

var RepeatableField = function RepeatableField(props) {
  var component = props.component,
      buttonComponent = props.buttonComponent,
      name = props.name,
      emptyField = props.emptyField,
      hideAddButton = props.hideAddButton,
      addButtonLabel = props.addButtonLabel,
      disabled = props.disabled,
      repeatableFieldSpecificProps = _objectWithoutProperties(props, ["component", "buttonComponent", "name", "emptyField", "hideAddButton", "addButtonLabel", "disabled"]);

  var FieldComponent = component;
  var ButtonComponent = buttonComponent;

  var _useField = (0, _formik.useField)(name),
      _useField2 = _slicedToArray(_useField, 1),
      field = _useField2[0];

  var currentRepeatableFieldValues = field.value ? field.value : {};
  return /*#__PURE__*/_react["default"].createElement(S.VerticalMargin, null, /*#__PURE__*/_react["default"].createElement(_formik.FieldArray, {
    name: name
  }, function (_ref) {
    var push = _ref.push,
        remove = _ref.remove;
    return /*#__PURE__*/_react["default"].createElement(_react["default"].Fragment, null, /*#__PURE__*/_react["default"].createElement(_Grid["default"], {
      container: true
    }, currentRepeatableFieldValues.map(function (fieldValues, index) {
      return /*#__PURE__*/_react["default"].createElement(FieldComponent, _extends({
        key: index,
        fieldValues: fieldValues,
        repeatableFieldNamePrefix: "".concat(name, "[").concat(index, "]"),
        disabled: disabled,
        removeButton: /*#__PURE__*/_react["default"].createElement(ButtonComponent, {
          onClick: function onClick() {
            return remove(index);
          }
        }, (0, _i18n.translate)(_i18n.keys.remove))
      }, repeatableFieldSpecificProps));
    })), !hideAddButton && !disabled && /*#__PURE__*/_react["default"].createElement(ButtonComponent, {
      onClick: function onClick() {
        return push(emptyField);
      }
    }, addButtonLabel));
  }));
};

RepeatableField.propTypes = {
  name: _propTypes["default"].string.isRequired,
  component: _propTypes["default"].func.isRequired,
  emptyField: _propTypes["default"].oneOfType([_propTypes["default"].string, _propTypes["default"].object]),
  buttonComponent: _propTypes["default"].func,
  addButtonLabel: _propTypes["default"].string,
  hideAddButton: _propTypes["default"].bool,
  disabled: _propTypes["default"].bool
};
RepeatableField.defaultProps = {
  buttonComponent: _Button["default"],
  addButtonLabel: (0, _i18n.translate)(_i18n.keys.add),
  hideAddButton: false,
  disabled: false
};
var _default = RepeatableField;
exports["default"] = _default;