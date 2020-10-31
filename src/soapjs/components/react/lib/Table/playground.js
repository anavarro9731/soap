"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _Table = _interopRequireDefault(require("./Table"));

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var data = [{
  label: 'abc',
  printer: 'Printer 1',
  paymentTerminalId: 'paymentTerminalId1'
}, {
  label: 'def',
  printer: 'Printer 2',
  paymentTerminalId: 'paymentTerminalId2'
}];
var columns = [{
  columnId: '1',
  header: 'Kiosk ID',
  render: function render(data) {
    return data.label;
  },
  width: '10%'
}, {
  columnId: '2',
  header: 'Printer',
  render: function render(data) {
    return data.printer;
  },
  width: 'auto'
}, {
  columnId: '3',
  header: 'Payment Information',
  render: function render(data) {
    return data.paymentTerminalId;
  }
}, {
  columnId: '4',
  header: 'Buttons',
  render: function render() {
    return /*#__PURE__*/_react["default"].createElement("input", {
      type: "button",
      text: "Button"
    });
  },
  width: '150px'
}];

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], {
  useDefaultFont: true
}, /*#__PURE__*/_react["default"].createElement("div", {
  style: {
    padding: '10px',
    height: '90%',
    width: '74%'
  }
}, /*#__PURE__*/_react["default"].createElement(_Table["default"], {
  columns: columns,
  data: data
}))), document.getElementById('content'));