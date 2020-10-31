"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var S = _interopRequireWildcard(require("./style"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var ButtonPanel = function ButtonPanel(props) {
  if (props.joinButtons) {
    var numberOfButtons = _react["default"].Children.count(props.children);

    return /*#__PURE__*/_react["default"].createElement(S.InlineButtons, null, _react["default"].Children.map(props.children, function (button, buttonIndex) {
      var isFirstButton = buttonIndex === 0;
      var isLastButton = buttonIndex === numberOfButtons - 1;

      if (isFirstButton) {
        return /*#__PURE__*/_react["default"].cloneElement(button, {
          borderRadiusRight: 'none'
        });
      } else if (isLastButton) {
        return /*#__PURE__*/_react["default"].cloneElement(button, {
          borderRadiusLeft: 'none',
          noLeftBorder: true
        });
      }

      return /*#__PURE__*/_react["default"].cloneElement(button, {
        borderRadiusLeft: 'none',
        borderRadiusRight: 'none',
        noLeftBorder: true
      });
    }));
  } else {
    return /*#__PURE__*/_react["default"].createElement(S.ButtonSpacing, {
      buttonSpacing: props.buttonSpacing
    }, props.children);
  }
};

ButtonPanel.propTypes = {
  children: _propTypes["default"].node.isRequired,
  joinButtons: _propTypes["default"].bool,
  buttonSpacing: _propTypes["default"].string
};
ButtonPanel.defaultProps = {
  joinButtons: false,
  buttonSpacing: '10px'
};
var _default = ButtonPanel;
exports["default"] = _default;