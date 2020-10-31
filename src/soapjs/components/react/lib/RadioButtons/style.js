"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Label = exports.RadioButton = exports.MarginTopAndButton = exports.Grid = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject4() {
  var data = _taggedTemplateLiteral(["\n  cursor: pointer;\n"]);

  _templateObject4 = function _templateObject4() {
    return data;
  };

  return data;
}

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  margin-right: 7px;\n  cursor: pointer;\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  margin: 10px 0;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: repeat(", ", 1fr);\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Grid = _styledComponents["default"].div(_templateObject(), function (props) {
  return props.radioButtonsPerRow;
});

exports.Grid = Grid;

var MarginTopAndButton = _styledComponents["default"].div(_templateObject2());

exports.MarginTopAndButton = MarginTopAndButton;

var RadioButton = _styledComponents["default"].input(_templateObject3());

exports.RadioButton = RadioButton;

var Label = _styledComponents["default"].label(_templateObject4());

exports.Label = Label;