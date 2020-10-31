"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _reactRouterDom = require("react-router-dom");

var _Link = _interopRequireDefault(require("./Link"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_react["default"].Fragment, null, /*#__PURE__*/_react["default"].createElement(_reactRouterDom.HashRouter, null, /*#__PURE__*/_react["default"].createElement(_reactRouterDom.Route, {
  path: "/",
  exact: true,
  component: function component() {
    return /*#__PURE__*/_react["default"].createElement("div", null, "Test Page");
  }
}), /*#__PURE__*/_react["default"].createElement(_Link["default"], {
  to: "/",
  textColour: "red",
  hoverTextColour: "blue"
}, "Test Link"))), document.getElementById('content'));