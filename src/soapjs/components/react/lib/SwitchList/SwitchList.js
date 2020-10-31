"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireWildcard(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _sorting = require("../../modules/utils/sorting");

var _Checkbox = _interopRequireDefault(require("../../infrastructure/Checkbox"));

var _Button = _interopRequireDefault(require("../../infrastructure/Button"));

var _MultiLineSelect = _interopRequireDefault(require("../MultiLineSelect"));

var S = _interopRequireWildcard(require("./style"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _iterableToArrayLimit(arr, i) { if (typeof Symbol === "undefined" || !(Symbol.iterator in Object(arr))) return; var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

function _toConsumableArray(arr) { return _arrayWithoutHoles(arr) || _iterableToArray(arr) || _unsupportedIterableToArray(arr) || _nonIterableSpread(); }

function _nonIterableSpread() { throw new TypeError("Invalid attempt to spread non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _iterableToArray(iter) { if (typeof Symbol !== "undefined" && Symbol.iterator in Object(iter)) return Array.from(iter); }

function _arrayWithoutHoles(arr) { if (Array.isArray(arr)) return _arrayLikeToArray(arr); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

var getSelectedOptionsFromSelectRef = function getSelectedOptionsFromSelectRef(selectRef) {
  return Array.prototype.map.call(selectRef.current.selectedOptions, function (option) {
    return {
      value: option.value,
      label: option.label
    };
  });
};

var removeSelectedOptionsFromList = function removeSelectedOptionsFromList(list, selectedOptions) {
  var selectedOptionValues = selectedOptions.map(function (option) {
    return option.value;
  });
  return list.filter(function (listItem) {
    return !selectedOptionValues.includes(listItem.value);
  });
};

var addSelectedOptionsToList = function addSelectedOptionsToList(list, selectedOptions) {
  var newList = [].concat(_toConsumableArray(list), _toConsumableArray(selectedOptions));
  return (0, _sorting.sortArrayByObjectPropertyAlphanumerically)(newList, function (option) {
    return option.value;
  });
};

var SwitchList = function SwitchList(props) {
  var _useState = (0, _react.useState)([]),
      _useState2 = _slicedToArray(_useState, 2),
      firstListOptions = _useState2[0],
      setFirstListOptions = _useState2[1];

  var _useState3 = (0, _react.useState)([]),
      _useState4 = _slicedToArray(_useState3, 2),
      secondListOptions = _useState4[0],
      setSecondListOptions = _useState4[1];

  (0, _react.useEffect)(function () {
    setFirstListOptions(props.initialFirstListOptions);
    setSecondListOptions(props.initialSecondListOptions);
  }, [props.firstListOptions, props.secondListOptions]);

  var handleRightArrowClick = function handleRightArrowClick() {
    var selectedOptions = getSelectedOptionsFromSelectRef(props.firstListRef);
    setFirstListOptions(removeSelectedOptionsFromList(firstListOptions, selectedOptions));
    setSecondListOptions(addSelectedOptionsToList(secondListOptions, selectedOptions));
  };

  var handleLeftArrowClick = function handleLeftArrowClick() {
    var selectedOptions = getSelectedOptionsFromSelectRef(props.secondListRef);
    setFirstListOptions(addSelectedOptionsToList(firstListOptions, selectedOptions));
    setSecondListOptions(removeSelectedOptionsFromList(secondListOptions, selectedOptions));
  };

  var handleCheckboxSelect = function handleCheckboxSelect(checkboxValue) {
    setCheckboxValue(checkboxValue);

    if (checkboxValue === true) {
      setSecondListOptions([].concat(_toConsumableArray(firstListOptions), _toConsumableArray(secondListOptions)));
      setFirstListOptions([]);
    }
  };

  var _useState5 = (0, _react.useState)(props.initialFirstListOptions.length === 0),
      _useState6 = _slicedToArray(_useState5, 2),
      checkboxValue = _useState6[0],
      setCheckboxValue = _useState6[1];

  var disabled = props.withSelectAllCheckbox && checkboxValue;
  var commonListProps = {
    visibleOptions: props.visibleOptionsInEachList,
    width: '100%',
    height: props.listHeight,
    disabled: disabled
  };
  var commonButtonProps = {
    width: 'fit-content',
    height: 'fit-content',
    fontSize: '28px',
    disabled: disabled
  };
  var CustomButton = props.buttonComponent;
  var CustomCheckbox = props.checkboxComponent;
  return /*#__PURE__*/_react["default"].createElement(S.SwitchListVerticalMargin, null, props.withSelectAllCheckbox && /*#__PURE__*/_react["default"].createElement(CustomCheckbox, {
    value: checkboxValue,
    onChange: handleCheckboxSelect,
    checkboxLabel: props.selectAllCheckboxLabel
  }), /*#__PURE__*/_react["default"].createElement(S.Lists, {
    listWidth: props.listWidth
  }, /*#__PURE__*/_react["default"].createElement(S.ListTitle, null, props.firstListTitle), /*#__PURE__*/_react["default"].createElement("div", null), /*#__PURE__*/_react["default"].createElement(S.ListTitle, null, props.secondListTitle), /*#__PURE__*/_react["default"].createElement(_MultiLineSelect["default"], _extends({
    options: firstListOptions,
    listRef: props.firstListRef
  }, commonListProps)), /*#__PURE__*/_react["default"].createElement(S.ArrowButtons, null, /*#__PURE__*/_react["default"].createElement(CustomButton, _extends({
    onClick: handleRightArrowClick
  }, commonButtonProps), "\u203A"), /*#__PURE__*/_react["default"].createElement(CustomButton, _extends({
    onClick: handleLeftArrowClick
  }, commonButtonProps), "\u2039")), /*#__PURE__*/_react["default"].createElement(_MultiLineSelect["default"], _extends({
    options: secondListOptions,
    listRef: props.secondListRef
  }, commonListProps))));
};

SwitchList.propTypes = {
  visibleOptionsInEachList: _propTypes["default"].number,
  initialFirstListOptions: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    value: _propTypes["default"].string.isRequired,
    label: _propTypes["default"].string.isRequired
  })).isRequired,
  initialSecondListOptions: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    value: _propTypes["default"].string.isRequired,
    label: _propTypes["default"].string.isRequired
  })).isRequired,
  listWidth: _propTypes["default"].string,
  buttonComponent: _propTypes["default"].func,
  checkboxComponent: _propTypes["default"].func,
  firstListTitle: _propTypes["default"].string,
  secondListTitle: _propTypes["default"].string,
  withSelectAllCheckbox: _propTypes["default"].bool,
  selectAllCheckboxLabel: _propTypes["default"].string,
  listHeight: _propTypes["default"].string,
  firstListRef: _propTypes["default"].object,
  secondListRef: _propTypes["default"].object
};
SwitchList.defaultProps = {
  visibleOptionsInEachList: 15,
  listWidth: 'auto',
  buttonComponent: _Button["default"],
  checkboxComponent: _Checkbox["default"],
  withSelectAllCheckbox: false,
  listHeight: '305px'
};
var _default = SwitchList;
exports["default"] = _default;