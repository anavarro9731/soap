"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireWildcard(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var S = _interopRequireWildcard(require("./style"));

var _Tab = _interopRequireDefault(require("./Tab"));

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

function _objectWithoutProperties(source, excluded) { if (source == null) return {}; var target = _objectWithoutPropertiesLoose(source, excluded); var key, i; if (Object.getOwnPropertySymbols) { var sourceSymbolKeys = Object.getOwnPropertySymbols(source); for (i = 0; i < sourceSymbolKeys.length; i++) { key = sourceSymbolKeys[i]; if (excluded.indexOf(key) >= 0) continue; if (!Object.prototype.propertyIsEnumerable.call(source, key)) continue; target[key] = source[key]; } } return target; }

function _objectWithoutPropertiesLoose(source, excluded) { if (source == null) return {}; var target = {}; var sourceKeys = Object.keys(source); var key, i; for (i = 0; i < sourceKeys.length; i++) { key = sourceKeys[i]; if (excluded.indexOf(key) >= 0) continue; target[key] = source[key]; } return target; }

var checkChildIsTab = function checkChildIsTab(tab) {
  return tab.type !== _Tab["default"] && console.error('Only the Tab component should be used directly underneath the Tabs component');
};

var mapChildrenToTabs = function mapChildrenToTabs(children, setActiveTabIndex, activeTabIndex, useFullWidthForTabs) {
  return _react["default"].Children.map(children, function (tab, tabIndex) {
    // We don't want the title property on the divs
    var _tab$props = tab.props,
        title = _tab$props.title,
        tabProps = _objectWithoutProperties(_tab$props, ["title"]);

    checkChildIsTab(tab);

    if ( /*#__PURE__*/_react["default"].isValidElement(tab)) {
      var isActiveTab = activeTabIndex === tabIndex;
      return /*#__PURE__*/_react["default"].createElement(S.Tab, _extends({
        useFullWidthForTabs: useFullWidthForTabs,
        isActiveTab: isActiveTab,
        onClick: function onClick() {
          return setActiveTabIndex(tabIndex);
        }
      }, tabProps), tab.props.title);
    }
  });
};

var getTabContent = function getTabContent(children, activeTabIndex) {
  return _react["default"].Children.map(children, function (tab, tabIndex) {
    var isActiveTab = activeTabIndex === tabIndex;
    return /*#__PURE__*/_react["default"].createElement(S.TabContent, {
      isActiveTab: isActiveTab
    }, tab);
  });
};

var setInitiallySelectedTab = function setInitiallySelectedTab(children, setActiveTabIndex) {
  return _react["default"].Children.map(children, function (tab, tabIndex) {
    if (tab.props.initiallySelected) {
      setActiveTabIndex(tabIndex);
    }
  });
};

var Tabs = function Tabs(props) {
  var _useState = (0, _react.useState)(0),
      _useState2 = _slicedToArray(_useState, 2),
      activeTabIndex = _useState2[0],
      setActiveTabIndex = _useState2[1];

  (0, _react.useEffect)(function () {
    setInitiallySelectedTab(props.children, setActiveTabIndex);
  }, []);
  var tabs = mapChildrenToTabs(props.children, setActiveTabIndex, activeTabIndex, props.useFullWidthForTabs);
  var tabContent = getTabContent(props.children, activeTabIndex);
  return /*#__PURE__*/_react["default"].createElement(S.TabsAndContent, null, /*#__PURE__*/_react["default"].createElement(S.Tabs, null, tabs), tabContent);
};

Tabs.propTypes = {
  children: _propTypes["default"].node.isRequired,
  useFullWidthForTabs: _propTypes["default"].bool
};
Tabs.defaultProps = {
  useFullWidthForTabs: false
};
Tabs.Tab = _Tab["default"];
var _default = Tabs;
exports["default"] = _default;