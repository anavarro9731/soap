"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _reactToastify = require("react-toastify");

require("react-toastify/dist/ReactToastify.css");

var _defaults = require("../../modules/style/defaults");

var S = _interopRequireWildcard(require("./style"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var ToastContainer = function ToastContainer(props) {
  return /*#__PURE__*/_react["default"].createElement(S.ToastContainer, props);
};

ToastContainer.propTypes = {
  position: _propTypes["default"].string,
  hideProgressBar: _propTypes["default"].bool,
  autoCloseTimeout: _propTypes["default"].oneOfType([_propTypes["default"].bool, _propTypes["default"].number]),
  width: _propTypes["default"].string,
  progressBarColour: _propTypes["default"].string,
  errorBackgroundColour: _propTypes["default"].string,
  errorTextColour: _propTypes["default"].string,
  errorTopBorderColour: _propTypes["default"].string,
  warningBackgroundColour: _propTypes["default"].string,
  warningTextColour: _propTypes["default"].string,
  warningTopBorderColour: _propTypes["default"].string,
  successBackgroundColour: _propTypes["default"].string,
  successTextColour: _propTypes["default"].string,
  successTopBorderColour: _propTypes["default"].string
};
ToastContainer.defaultProps = {
  position: _reactToastify.toast.POSITION.TOP_RIGHT,
  hideProgressBar: false,
  autoCloseTimeout: false,
  width: '40%',
  progressBarColour: _defaults.defaultBorderColour,
  errorBackgroundColour: '#f2dede',
  errorTextColour: '#a94442',
  errorTopBorderColour: '#a94442',
  warningBackgroundColour: _defaults.defaultLightBackgroundColour,
  warningTextColour: _defaults.defaultTextColour,
  warningTopBorderColour: _defaults.defaultHighlightColour,
  successBackgroundColour: _defaults.defaultLightBackgroundColour,
  successTextColour: _defaults.defaultTextColour,
  successTopBorderColour: _defaults.defaultHighlightColour
};
var _default = ToastContainer;
exports["default"] = _default;