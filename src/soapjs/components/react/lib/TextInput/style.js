"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.TextInput = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  padding: 9px;\n  box-sizing: border-box;\n  width: ", ";\n  border: ", ";\n  ", "\n  border-radius: 4px;\n  font-size: 15px;\n\n  &:focus {\n    outline: none;\n  }\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var TextInput = _styledComponents["default"].input(_templateObject(), function (props) {
  return props.width;
}, function (props) {
  return props.border;
}, function (props) {
  return props.disabled && "background-color: #ededed";
});

exports.TextInput = TextInput;