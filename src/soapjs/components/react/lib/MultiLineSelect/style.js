"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Option = exports.Select = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  padding: 2px 5px;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  width: ", ";\n  height: ", ";\n  min-width: ", ";\n  font-size: 16px;\n  padding: 3px 0;\n\n  &:focus {\n    outline: none;\n  }\n\n  &:disabled {\n    background-color: #ededed;\n  }\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Select = _styledComponents["default"].select(_templateObject(), function (props) {
  return props.width;
}, function (props) {
  return props.height;
}, function (props) {
  return props.width;
});

exports.Select = Select;

var Option = _styledComponents["default"].option(_templateObject2());

exports.Option = Option;