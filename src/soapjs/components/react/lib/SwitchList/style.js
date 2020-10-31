"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ListTitle = exports.ArrowButtons = exports.Lists = exports.SwitchListVerticalMargin = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject4() {
  var data = _taggedTemplateLiteral(["\n  padding: 15px 0;\n"]);

  _templateObject4 = function _templateObject4() {
    return data;
  };

  return data;
}

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-rows: 1fr 1fr;\n  justify-items: center;\n  align-items: center;\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: ", " 100px ", ";\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  margin: 10px 0;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var SwitchListVerticalMargin = _styledComponents["default"].div(_templateObject());

exports.SwitchListVerticalMargin = SwitchListVerticalMargin;

var Lists = _styledComponents["default"].div(_templateObject2(), function (props) {
  return props.listWidth;
}, function (props) {
  return props.listWidth;
});

exports.Lists = Lists;

var ArrowButtons = _styledComponents["default"].div(_templateObject3());

exports.ArrowButtons = ArrowButtons;

var ListTitle = _styledComponents["default"].div(_templateObject4());

exports.ListTitle = ListTitle;