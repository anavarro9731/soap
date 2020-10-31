"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

var _react = _interopRequireWildcard(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _SwitchList = _interopRequireDefault(require("./SwitchList"));

var _refs = require("../../modules/utils/refs");

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

var initialFirstListOptions = [{
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
var initialSecondListOptions = [{
  value: 'value 11',
  label: 'Label 11'
}, {
  value: 'value 12',
  label: 'Label 12'
}, {
  value: 'value 13',
  label: 'Label 13'
}, {
  value: 'value 14',
  label: 'Label 14'
}, {
  value: 'value 15',
  label: 'Label 15'
}, {
  value: 'value 16',
  label: 'Label 16'
}, {
  value: 'value 17',
  label: 'Label 17'
}, {
  value: 'value 18',
  label: 'Label 18'
}, {
  value: 'value 19',
  label: 'Label 19'
}, {
  value: 'value 20',
  label: 'Label 20'
}, {
  value: 'value 21',
  label: 'Label 21'
}, {
  value: 'value 22',
  label: 'Label 22'
}];

var ComponentUsingSwitchList = function ComponentUsingSwitchList() {
  var firstListRef = (0, _react.useRef)();
  var secondListRef = (0, _react.useRef)();

  var handleClick = function handleClick() {
    console.log('First List: ', (0, _refs.getSelectRefOptions)(firstListRef));
    console.log('Second List: ', (0, _refs.getSelectRefOptions)(secondListRef));
  };

  return /*#__PURE__*/_react["default"].createElement(_react["default"].Fragment, null, /*#__PURE__*/_react["default"].createElement("button", {
    style: {
      marginBottom: '50px'
    },
    onClick: handleClick
  }, "Console.log list values"), /*#__PURE__*/_react["default"].createElement(_SwitchList["default"], {
    initialFirstListOptions: initialFirstListOptions,
    initialSecondListOptions: initialSecondListOptions,
    firstListRef: firstListRef,
    secondListRef: secondListRef,
    firstListTitle: "First List",
    secondListTitle: "Second List"
  }));
};

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], {
  useDefaultFont: true
}, /*#__PURE__*/_react["default"].createElement(ComponentUsingSwitchList, null)), document.getElementById('content'));