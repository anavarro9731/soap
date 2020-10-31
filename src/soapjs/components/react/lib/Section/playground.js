"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _Section = _interopRequireDefault(require("./Section"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_Section["default"], {
  title: "Test Title",
  rightAlignedContent: "Right aligned content",
  startExpanded: true
}, "aa"), document.getElementById('content'));