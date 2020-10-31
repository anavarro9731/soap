"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _AppWrapper = _interopRequireDefault(require("./AppWrapper"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], null, /*#__PURE__*/_react["default"].createElement("div", null, "Test")), document.getElementById('content'));