"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.TabContent = exports.Tab = exports.Tabs = exports.TabsAndContent = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject4() {
  var data = _taggedTemplateLiteral(["\n  border: 1px solid #d2d2d2;\n  ", "\n  padding: 10px;\n"]);

  _templateObject4 = function _templateObject4() {
    return data;
  };

  return data;
}

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  vertical-align: middle;\n  display: ", ";\n  text-align: center;\n  padding: 10px;\n  width: 12%;\n  height: 100%;\n  cursor: pointer;\n  border: 1px solid #d2d2d2;\n  border-bottom: none;\n  margin-right: -1px;\n  margin-bottom: -1px;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  color: ", ";\n  ", "\n\n  :hover {\n    background-color: ", ";\n    color: ", ";\n  }\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  width: 100%;\n  text-align: center;\n  display: table;\n  table-layout: fixed;\n  border-collapse: collapse;\n  box-sizing: border-box;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  width: 100%;\n  height: 100%;\n  display: grid;\n  grid-template-rows: auto 1fr;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var TabsAndContent = _styledComponents["default"].div(_templateObject());

exports.TabsAndContent = TabsAndContent;

var Tabs = _styledComponents["default"].div(_templateObject2());

exports.Tabs = Tabs;

var Tab = _styledComponents["default"].div(_templateObject3(), function (props) {
  return props.useFullWidthForTabs ? 'table-cell' : 'inline-block';
}, function (props) {
  return props.isActiveTab ? props.hoverAndActiveTabtextColour : props.tabTextColour;
}, function (props) {
  return props.isActiveTab && "background-color: ".concat(props.hoverAndActiveTabBackgroundColour, ";\n    ");
}, function (props) {
  return props.hoverAndActiveTabBackgroundColour;
}, function (props) {
  return props.hoverAndActiveTabtextColour;
});

exports.Tab = Tab;

var TabContent = _styledComponents["default"].div(_templateObject4(), function (props) {
  return !props.isActiveTab && "display: none;";
});

exports.TabContent = TabContent;