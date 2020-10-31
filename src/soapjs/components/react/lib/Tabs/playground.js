"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _Tabs = _interopRequireDefault(require("./Tabs"));

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

var commonStylingProps = {
  hoverAndActiveTabBackgroundColour: '#3b3f4a'
};

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], null, /*#__PURE__*/_react["default"].createElement("div", {
  style: {
    padding: '10px',
    height: '90%',
    width: '74%'
  }
}, /*#__PURE__*/_react["default"].createElement(_Tabs["default"], null, /*#__PURE__*/_react["default"].createElement(_Tabs["default"].Tab, _extends({
  title: "Product and Something"
}, commonStylingProps), /*#__PURE__*/_react["default"].createElement("div", null, "some text")), /*#__PURE__*/_react["default"].createElement(_Tabs["default"].Tab, _extends({
  initiallySelected: true,
  title: "Hardware"
}, commonStylingProps), /*#__PURE__*/_react["default"].createElement("div", null, /*#__PURE__*/_react["default"].createElement("input", {
  type: "text"
}))), /*#__PURE__*/_react["default"].createElement(_Tabs["default"].Tab, _extends({
  title: "Information"
}, commonStylingProps), /*#__PURE__*/_react["default"].createElement("div", null))))), document.getElementById('content'));