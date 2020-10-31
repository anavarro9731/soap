"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _LoadingOverlay = _interopRequireDefault(require("./LoadingOverlay"));

var _apiRequest = require("../../modules/hooks/api-request");

var _soap = require("../../modules/soap");

var _Accordion = _interopRequireDefault(require("../Accordion"));

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var testQuery = {
  schema: 'testQuery',
  pointlessprop: '2342342342'
};

var QueryCall = function QueryCall() {
  (0, _soap.mockEvent)(testQuery, {
    testResponse: 'testResponse'
  });
  (0, _apiRequest.useQuery)(testQuery);
  return /*#__PURE__*/_react["default"].createElement("div", null);
};

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], null, /*#__PURE__*/_react["default"].createElement(_LoadingOverlay["default"], {
  requests: [testQuery]
}, /*#__PURE__*/_react["default"].createElement(QueryCall, null), /*#__PURE__*/_react["default"].createElement(_Accordion["default"], {
  sections: [{
    id: '1',
    title: 'Fanta',
    rightAlignedContent: 'X',
    content: 'Failed because of a reason'
  }, {
    id: '2',
    title: 'Chicken nuggets',
    rightAlignedContent: '✔'
  }]
}))), document.getElementById('content'));