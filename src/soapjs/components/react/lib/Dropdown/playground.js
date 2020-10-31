"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _Dropdown = _interopRequireDefault(require("./Dropdown"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var options = [{
  value: 'chocolate',
  label: 'Chocolate'
}, {
  value: 'vanilla',
  label: 'Vanilla'
}];

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_Dropdown["default"], {
  options: options,
  value: "chocolate",
  width: "500px",
  onChange: function onChange(value) {
    return console.log(value);
  }
}), document.getElementById('content'));