"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _propTypes = _interopRequireDefault(require("prop-types"));

var _defaults = require("../../modules/style/defaults");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var Tab = function Tab(props) {
  return props.children;
};

Tab.propTypes = {
  children: _propTypes["default"].node,
  title: _propTypes["default"].string.isRequired,
  tabTextColour: _propTypes["default"].string,
  hoverAndActiveTabBackgroundColour: _propTypes["default"].string,
  hoverAndActiveTabtextColour: _propTypes["default"].string,
  initiallySelected: _propTypes["default"].bool
};
Tab.defaultProps = {
  tabTextColour: _defaults.defaultTextColour,
  hoverAndActiveTabBackgroundColour: _defaults.defaultHighlightColour,
  hoverAndActiveTabtextColour: _defaults.defaultLightTextColour,
  initiallySelected: false
};
var _default = Tab;
exports["default"] = _default;