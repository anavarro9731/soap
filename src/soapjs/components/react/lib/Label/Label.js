"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _propTypes = _interopRequireDefault(require("prop-types"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var Label = function Label(props) {
  return props.value || null;
};

Label.propTypes = {
  value: _propTypes["default"].node
};
var _default = Label;
exports["default"] = _default;