"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Button = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  background-color: ", ";\n  cursor: pointer;\n  color: ", ";\n  padding: ", ";\n  font-size: ", ";\n  border: ", ";\n  border-top-left-radius: ", ";\n  border-bottom-left-radius: ", ";\n  border-top-right-radius: ", ";\n  border-bottom-right-radius: ", ";\n  ", "\n  height: ", ";\n  width: ", ";\n  ", "\n\n  &:focus {\n    outline: none;\n  }\n\n  ", "\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Button = _styledComponents["default"].button(_templateObject(), function (props) {
  return props.disabled ? props.disabledBackgroundColour : props.backgroundColour;
}, function (props) {
  return props.colour;
}, function (props) {
  return "".concat(props.verticalPadding, " ").concat(props.horizontalPadding);
}, function (props) {
  return props.fontSize;
}, function (props) {
  return props.border;
}, function (props) {
  return props.borderRadiusLeft;
}, function (props) {
  return props.borderRadiusLeft;
}, function (props) {
  return props.borderRadiusRight;
}, function (props) {
  return props.borderRadiusRight;
}, function (props) {
  return props.noLeftBorder && 'border-left: none;';
}, function (props) {
  return props.height;
}, function (props) {
  return props.width;
}, function (props) {
  return props.disabled && 'pointer-events: none;';
}, function (props) {
  return props.hoverBackgroundColour && "&:hover {\n    background-color: ".concat(props.hoverBackgroundColour, ";\n  }");
});

exports.Button = Button;