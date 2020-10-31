"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireWildcard(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _defaults = require("../../modules/style/defaults");

var _positions = require("./positions");

var _Button = _interopRequireDefault(require("../Button"));

var S = _interopRequireWildcard(require("./style"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _iterableToArrayLimit(arr, i) { if (typeof Symbol === "undefined" || !(Symbol.iterator in Object(arr))) return; var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

var renderLink = function renderLink(option, props) {
  var commonLinkProps = {
    key: option.label,
    colour: props.textColour,
    backgroundColour: props.backgroundColour,
    hoverBackgroundColour: props.hoverBackgroundColour
  };
  return option.route ? /*#__PURE__*/_react["default"].createElement(S.ReactRouterLink, _extends({}, commonLinkProps, {
    to: option.route
  }), option.label) : /*#__PURE__*/_react["default"].createElement(S.Link, _extends({}, commonLinkProps, {
    onClick: option.onClick
  }), option.label);
};

var DropdownButton = function DropdownButton(props) {
  var _useState = (0, _react.useState)(false),
      _useState2 = _slicedToArray(_useState, 2),
      isDropdownOpen = _useState2[0],
      setIsDropdownOpen = _useState2[1];

  var dropdownButtonRef = (0, _react.useRef)();
  (0, _react.useEffect)(function () {
    var handleClickOutside = function handleClickOutside(event) {
      if (dropdownButtonRef.current && !dropdownButtonRef.current.contains(event.target) && isDropdownOpen) {
        setIsDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return function () {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  });
  var CustomButtonComponent = props.buttonComponent;
  return /*#__PURE__*/_react["default"].createElement(S.Container, {
    ref: dropdownButtonRef
  }, /*#__PURE__*/_react["default"].createElement(CustomButtonComponent, _extends({
    onClick: function onClick() {
      return setIsDropdownOpen(!isDropdownOpen);
    }
  }, props.additionalButtonProps), props.buttonText), isDropdownOpen && /*#__PURE__*/_react["default"].createElement(S.Links, {
    border: props.border,
    position: props.position
  }, props.options.map(function (option) {
    return renderLink(option, props);
  })));
};

DropdownButton.propTypes = {
  buttonComponent: _propTypes["default"].func,
  textColour: _propTypes["default"].string,
  backgroundColour: _propTypes["default"].string,
  border: _propTypes["default"].string,
  hoverBackgroundColour: _propTypes["default"].string,
  buttonText: _propTypes["default"].node,
  options: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    label: _propTypes["default"].string.isRequired,
    route: _propTypes["default"].string,
    onClick: _propTypes["default"].func
  })),
  position: _propTypes["default"].string,
  additionalButtonProps: _propTypes["default"].object
};
DropdownButton.defaultProps = {
  buttonComponent: _Button["default"],
  textColour: _defaults.defaultLightTextColour,
  backgroundColour: _defaults.defaultPrimaryBackgroundColour,
  position: _positions.POSITIONS.RIGHT,
  additionalButtonProps: {},
  border: 'none'
};
DropdownButton.POSITIONS = _positions.POSITIONS;
var _default = DropdownButton;
exports["default"] = _default;