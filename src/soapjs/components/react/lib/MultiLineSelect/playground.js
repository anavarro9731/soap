"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _MultiLineSelect = _interopRequireDefault(require("./MultiLineSelect"));

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var options = [{
  value: 'value 1',
  label: 'Label 1'
}, {
  value: 'value 2',
  label: 'Label 2'
}, {
  value: 'value 3',
  label: 'Label 3'
}, {
  value: 'value 4',
  label: 'Label 4'
}, {
  value: 'value 5',
  label: 'Label 5'
}, {
  value: 'value 6',
  label: 'Label 6'
}, {
  value: 'value 7',
  label: 'Label 7'
}, {
  value: 'value 8',
  label: 'Label 8'
}, {
  value: 'value 9',
  label: 'Label 9'
}, {
  value: 'value 10',
  label: 'Label 10'
}];

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], {
  useDefaultFont: true
}, /*#__PURE__*/_react["default"].createElement(_MultiLineSelect["default"], {
  options: options
})), document.getElementById('content'));