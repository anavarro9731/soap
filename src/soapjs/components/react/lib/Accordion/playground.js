"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _Accordion = _interopRequireDefault(require("./Accordion"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_Accordion["default"], {
  sections: [{
    id: '1',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '2',
    title: 'Chicken nuggets',
    rightAlignedContent: '✔'
  }, {
    id: '3',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '4',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '5',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '6',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '7',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '8',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }]
}), document.getElementById('content'));