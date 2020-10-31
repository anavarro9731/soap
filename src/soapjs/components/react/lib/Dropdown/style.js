"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Select = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

var _reactSelect = _interopRequireDefault(require("react-select"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  width: ", ";\n  float: ", ";\n  min-width: ", ";\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Select = (0, _styledComponents["default"])(_reactSelect["default"])(_templateObject(), function (props) {
  return props.width;
}, function (props) {
  return props["float"];
}, function (props) {
  return props.minWidth;
});
exports.Select = Select;