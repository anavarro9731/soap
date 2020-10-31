"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Label = exports.Checkbox = exports.CheckboxWithLabel = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  padding-", ": 15px;\n  white-space: nowrap;\n  font-weight: bold;\n  ", "\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  width: 25px;\n  height: 25px;\n  margin: 0px;\n  ", "\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: auto 1fr;\n  align-items: center;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var CheckboxWithLabel = _styledComponents["default"].div(_templateObject());

exports.CheckboxWithLabel = CheckboxWithLabel;

var Checkbox = _styledComponents["default"].input(_templateObject2(), function (props) {
  return !props.disabled && 'cursor: pointer;';
});

exports.Checkbox = Checkbox;

var Label = _styledComponents["default"].label(_templateObject3(), function (props) {
  return props.labelOnLeftSide ? 'right' : 'left';
}, function (props) {
  return !props.disabled && 'cursor: pointer;';
});

exports.Label = Label;