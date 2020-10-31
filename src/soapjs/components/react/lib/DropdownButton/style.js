"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.Link = exports.ReactRouterLink = exports.Links = exports.Container = void 0;

var _react = _interopRequireDefault(require("react"));

var _styledComponents = _interopRequireDefault(require("styled-components"));

var _positions = require("./positions");

var _reactRouterDom = require("react-router-dom");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject4() {
  var data = _taggedTemplateLiteral(["\n  color: ", ";\n  background-color: ", ";\n  padding: 5px 15px;\n  text-decoration: none;\n  cursor: pointer;\n\n  &:hover {\n    background-color: ", ";\n  }\n"]);

  _templateObject4 = function _templateObject4() {
    return data;
  };

  return data;
}

function _objectWithoutProperties(source, excluded) { if (source == null) return {}; var target = _objectWithoutPropertiesLoose(source, excluded); var key, i; if (Object.getOwnPropertySymbols) { var sourceSymbolKeys = Object.getOwnPropertySymbols(source); for (i = 0; i < sourceSymbolKeys.length; i++) { key = sourceSymbolKeys[i]; if (excluded.indexOf(key) >= 0) continue; if (!Object.prototype.propertyIsEnumerable.call(source, key)) continue; target[key] = source[key]; } } return target; }

function _objectWithoutPropertiesLoose(source, excluded) { if (source == null) return {}; var target = {}; var sourceKeys = Object.keys(source); var key, i; for (i = 0; i < sourceKeys.length; i++) { key = sourceKeys[i]; if (excluded.indexOf(key) >= 0) continue; target[key] = source[key]; } return target; }

function _templateObject3() {
  var data = _taggedTemplateLiteral(["\n  color: ", ";\n  background-color: ", ";\n  padding: 5px 15px;\n  text-decoration: none;\n\n  &:hover {\n    background-color: ", ";\n  }\n"]);

  _templateObject3 = function _templateObject3() {
    return data;
  };

  return data;
}

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  display: grid;\n  grid-template-columns: max-content;\n  position: absolute;\n   ", "\n  border: ", ";\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  width: fit-content;\n  display: inline-block;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var Container = _styledComponents["default"].div(_templateObject());

exports.Container = Container;

var Links = _styledComponents["default"].div(_templateObject2(), function (props) {
  return (0, _positions.isPositionLeft)(props.position) && 'transform: translateX(-80%);';
}, function (props) {
  return props.border;
});

exports.Links = Links;
var ReactRouterLink = (0, _styledComponents["default"])(function (_ref) {
  var colour = _ref.colour,
      backgroundColour = _ref.backgroundColour,
      hoverBackgroundColour = _ref.hoverBackgroundColour,
      rest = _objectWithoutProperties(_ref, ["colour", "backgroundColour", "hoverBackgroundColour"]);

  return /*#__PURE__*/_react["default"].createElement(_reactRouterDom.Link, rest);
})(_templateObject3(), function (props) {
  return props.colour;
}, function (props) {
  return props.backgroundColour;
}, function (props) {
  return props.hoverBackgroundColour;
});
exports.ReactRouterLink = ReactRouterLink;

var Link = _styledComponents["default"].div(_templateObject4(), function (props) {
  return props.colour;
}, function (props) {
  return props.backgroundColour;
}, function (props) {
  return props.hoverBackgroundColour;
});

exports.Link = Link;