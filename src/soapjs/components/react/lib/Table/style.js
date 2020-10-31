"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.CellContent = exports.Cell = exports.Table = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  overflow: hidden;\n  text-overflow: ellipsis;\n  padding: ", ";\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  outline: 1px solid ", ";\n  color: ", ";\n  background-color: ", ";\n  display: flex;\n  align-items: center;\n  box-sizing: border-box;\n  overflow: hidden;\n  white-space: nowrap;\n  ", "\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: ", ";\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Table = _styledComponents["default"].div(_templateObject(), function (props) {
  return props.columnWidths;
});

exports.Table = Table;

var Cell = _styledComponents["default"].div(_templateObject2(), function (props) {
  return props.borderColour;
}, function (props) {
  return props.textColour;
}, function (props) {
  return props.backgroundColour;
}, function (props) {
  return props.fontWeight && "font-weight: ".concat(props.fontWeight);
});

exports.Cell = Cell;

var CellContent = _styledComponents["default"].span(_templateObject3(), function (props) {
  return props.padding;
});

exports.CellContent = CellContent;