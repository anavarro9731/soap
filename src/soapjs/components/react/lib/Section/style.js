"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Section = exports.Content = exports.RightAlignedContent = exports.Title = exports.Chevron = exports.ClickToToggleExpand = exports.Heading = void 0;

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject7() {
  var data = _taggedTemplateLiteral(["\n  margin: 10px 0;\n"]);

  _templateObject7 = function _templateObject7() {
    return data;
  };

  return data;
}

function _templateObject6() {
  var data = _taggedTemplateLiteral(["\n  font-size: 14px;\n  word-wrap: break-word;\n  padding: 0 60px;\n  ", "\n"]);

  _templateObject6 = function _templateObject6() {
    return data;
  };

  return data;
}

function _templateObject5() {
  var data = _taggedTemplateLiteral(["\n  justify-self: right;\n  padding-right: 30px;\n"]);

  _templateObject5 = function _templateObject5() {
    return data;
  };

  return data;
}

function _templateObject4() {
  var data = _taggedTemplateLiteral(["\n  font-size: 16px;\n  cursor: pointer;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n"]);

  _templateObject4 = function _templateObject4() {
    return data;
  };

  return data;
}

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  width: 17px;\n  height: 17px;\n  margin: 0 auto;\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: 50px 1fr;\n  align-items: center;\n  cursor: pointer;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: 75% 25%;\n  padding-bottom: 15px;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Heading = _styledComponents["default"].div(_templateObject());

exports.Heading = Heading;

var ClickToToggleExpand = _styledComponents["default"].div(_templateObject2());

exports.ClickToToggleExpand = ClickToToggleExpand;

var Chevron = _styledComponents["default"].img(_templateObject3());

exports.Chevron = Chevron;

var Title = _styledComponents["default"].div(_templateObject4());

exports.Title = Title;

var RightAlignedContent = _styledComponents["default"].div(_templateObject5());

exports.RightAlignedContent = RightAlignedContent;

var Content = _styledComponents["default"].div(_templateObject6(), function (props) {
  return props.isSectionExpanded && 'padding-bottom: 15px;';
});

exports.Content = Content;

var Section = _styledComponents["default"].div(_templateObject7());

exports.Section = Section;