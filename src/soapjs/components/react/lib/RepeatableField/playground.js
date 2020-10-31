"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _RepeatableField = _interopRequireDefault(require("./RepeatableField"));

var _formik = require("formik");

var _TextInput = _interopRequireDefault(require("../TextInput"));

var _Button = _interopRequireDefault(require("../Button"));

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

var _Field = _interopRequireDefault(require("../Field"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _iterableToArrayLimit(arr, i) { if (typeof Symbol === "undefined" || !(Symbol.iterator in Object(arr))) return; var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

var ExampleField = function ExampleField(props) {
  var _useField = (0, _formik.useField)(props.name),
      _useField2 = _slicedToArray(_useField, 2),
      field = _useField2[0],
      meta = _useField2[1];

  return /*#__PURE__*/_react["default"].createElement(_Field["default"], _extends({}, props, {
    field: field
  }));
};

var RepeatableInputField = function RepeatableInputField(props) {
  return /*#__PURE__*/_react["default"].createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr 1fr'
    }
  }, /*#__PURE__*/_react["default"].createElement(ExampleField, {
    name: "".concat(props.repeatableFieldNamePrefix, ".country"),
    component: _TextInput["default"],
    width: "400px"
  }), /*#__PURE__*/_react["default"].createElement(ExampleField, {
    name: "".concat(props.repeatableFieldNamePrefix, ".age"),
    component: _TextInput["default"],
    width: "400px"
  }), /*#__PURE__*/_react["default"].createElement("span", null, props.removeButton));
};

var emptyPersonField = {
  age: '',
  country: ''
};
var personFieldValues = [{
  age: '10',
  country: 'United Kingdom'
}, {
  age: '12',
  country: 'France'
}];

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], {
  useDefaultFont: true
}, /*#__PURE__*/_react["default"].createElement(_formik.Formik, {
  onSubmit: function onSubmit(formValues) {
    return console.log(formValues);
  },
  initialValues: {
    testName: personFieldValues.length ? personFieldValues : emptyPersonField
  }
}, function (_ref) {
  var values = _ref.values;
  return /*#__PURE__*/_react["default"].createElement(_formik.Form, null, /*#__PURE__*/_react["default"].createElement(_RepeatableField["default"], {
    name: "testName",
    component: RepeatableInputField,
    currentFormValues: values,
    emptyField: emptyPersonField
  }), /*#__PURE__*/_react["default"].createElement("div", {
    style: {
      height: '10px'
    }
  }), /*#__PURE__*/_react["default"].createElement(_Button["default"], {
    type: "submit"
  }, "Console.log form values"));
})), document.getElementById('content'));