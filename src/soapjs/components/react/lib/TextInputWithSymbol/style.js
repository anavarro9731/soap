"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Symbol = exports.Grid = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

var _defaults = require("../../modules/style/defaults");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  padding: 6px;\n  border-", ": 1px solid ", ";\n  font-weight: bold;\n  color: ", ";\n  font-size: 18px;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: ", ";\n  align-items: center;\n  border: 1px solid ", ";\n  background-color: ", ";\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Grid = _styledComponents["default"].div(_templateObject(), function (props) {
  return props.rightAlignSymbol ? '1fr auto' : 'auto 1fr';
}, _defaults.defaultBorderColour, _defaults.defaultLightBackgroundColour);

exports.Grid = Grid;

var _Symbol = _styledComponents["default"].div(_templateObject2(), function (props) {
  return props.rightAlignSymbol ? 'left' : 'right';
}, _defaults.defaultBorderColour, function (props) {
  return props.symbolColour;
});

exports.Symbol = _Symbol;