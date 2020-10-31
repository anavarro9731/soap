"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ToastContainer = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

var _reactToastify = require("react-toastify");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  width: ", ";\n\n  button[aria-label='close'] {\n    color: #000;\n  }\n\n  .Toastify__toast-container {\n  }\n\n  .Toastify__toast {\n  }\n\n  .Toastify__toast--error {\n    background-color: ", ";\n    color: ", ";\n    border-top: 7px solid ", ";\n  }\n  .Toastify__toast--warning {\n    background-color: ", ";\n    color: ", ";\n    border-top: 7px solid ", ";\n  }\n  .Toastify__toast--success {\n    background-color: ", ";\n    color: ", ";\n    border-top: 7px solid ", ";\n  }\n  .Toastify__toast-body {\n    margin: 10px;\n  }\n  .Toastify__progress-bar {\n    background-color: ", ";\n  }\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var ToastContainer = (0, _styledComponents["default"])(_reactToastify.ToastContainer).attrs(function (props) {
  return {
    position: props.position,
    hideProgressBar: props.hideProgressBar,
    autoClose: props.autoCloseTimeout
  };
})(_templateObject(), function (props) {
  return props.width;
}, function (props) {
  return props.errorBackgroundColour;
}, function (props) {
  return props.errorTextColour;
}, function (props) {
  return props.errorTopBorderColour;
}, function (props) {
  return props.warningBackgroundColour;
}, function (props) {
  return props.warningTextColour;
}, function (props) {
  return props.warningTopBorderColour;
}, function (props) {
  return props.successBackgroundColour;
}, function (props) {
  return props.successTextColour;
}, function (props) {
  return props.successTopBorderColour;
}, function (props) {
  return props.progressBarColour;
});
exports.ToastContainer = ToastContainer;