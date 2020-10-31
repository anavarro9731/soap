"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _reactRouterDom = require("react-router-dom");

var _DropdownButton = _interopRequireDefault(require("./DropdownButton"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var options = [{
  onClick: function onClick() {
    return console.log('first option clicked');
  },
  label: 'Edit Page'
}, {
  route: '/test',
  label: 'Add Page'
}];

var Dropdown = function Dropdown() {
  return /*#__PURE__*/_react["default"].createElement(_DropdownButton["default"], {
    options: options,
    buttonText: "Click here",
    width: "500px",
    position: _DropdownButton["default"].POSITIONS.LEFT
  });
};

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_reactRouterDom.HashRouter, null, /*#__PURE__*/_react["default"].createElement(_reactRouterDom.Route, {
  path: "/",
  exact: true,
  component: Dropdown
})), document.getElementById('content'));