"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.RightAlignedIcons = exports.MenuItem = exports.MenuItems = exports.MenuBar = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject4() {
  var data = _taggedTemplateLiteral(["\n  text-align: right;\n  font-size: 1em;\n"]);

  _templateObject4 = function _templateObject4() {
    return data;
  };

  return data;
}

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  padding: 0 15px;\n  display: inline-block;\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  text-align: right;\n  font-size: 1.05em;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: 8% 68% 24%;\n  align-items: center;\n  background-color: #3b3f4a;\n  height: 100%;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var MenuBar = _styledComponents["default"].div(_templateObject());

exports.MenuBar = MenuBar;

var MenuItems = _styledComponents["default"].div(_templateObject2());

exports.MenuItems = MenuItems;

var MenuItem = _styledComponents["default"].div(_templateObject3());

exports.MenuItem = MenuItem;

var RightAlignedIcons = _styledComponents["default"].div(_templateObject4());

exports.RightAlignedIcons = RightAlignedIcons;