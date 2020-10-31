"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ButtonSpacing = exports.InlineButtons = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  & > * {\n    margin-right: ", ";\n  }\n\n  & > *:last-child {\n    margin-right: 0;\n  }\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: inline-block;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var InlineButtons = _styledComponents["default"].div(_templateObject());

exports.InlineButtons = InlineButtons;

var ButtonSpacing = _styledComponents["default"].div(_templateObject2(), function (props) {
  return props.buttonSpacing;
});

exports.ButtonSpacing = ButtonSpacing;