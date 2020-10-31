"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _ToastContainer = _interopRequireDefault(require("../ToastContainer"));

var _soap = require("../../modules/soap");

var _eventNames = require("../../modules/classes/eventNames");

var _event = require("../../modules/hooks/event");

var _toast = require("../ToastContainer/toast");

var S = _interopRequireWildcard(require("./style"));

require("./font.css");

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var withDefaultFont = function withDefaultFont(component) {
  return /*#__PURE__*/_react["default"].createElement("div", {
    style: {
      height: '100%',
      fontFamily: 'Muli'
    }
  }, component);
};

var AppWrapper = function AppWrapper(props) {
  (0, _event.useSubscribeToApiEvent)(_eventNames.EVENTS.ApiErrorOccurred, function (apiResponse) {
    return (0, _toast.displayToast)(_toast.TOAST_TYPES.ERROR, apiResponse.message);
  }, _soap.bus.channels.errors);

  var app = /*#__PURE__*/_react["default"].createElement(_react["default"].Fragment, null, props.setHtmlAndBodyFullHeight && /*#__PURE__*/_react["default"].createElement(S.HtmlBodyAndContentFullHeight, null), /*#__PURE__*/_react["default"].createElement(_ToastContainer["default"], null), props.children);

  return props.useDefaultFont ? withDefaultFont(app) : app;
};

AppWrapper.propTypes = {
  setHtmlAndBodyFullHeight: _propTypes["default"].bool,
  useDefaultFont: _propTypes["default"].bool
};
AppWrapper.defaultProps = {
  setHtmlAndBodyFullHeight: true,
  useDefaultFont: false
};
var _default = AppWrapper;
exports["default"] = _default;