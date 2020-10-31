"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireWildcard(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _Accordion = _interopRequireDefault(require("../Accordion"));

var _Button = _interopRequireDefault(require("../Button"));

var _ButtonPanel = _interopRequireDefault(require("../ButtonPanel"));

var _Grid = _interopRequireDefault(require("@material-ui/core/Grid"));

var _i18n = require("../../modules/i18n");

var _formik = require("formik");

var _Checkbox = _interopRequireDefault(require("../Checkbox"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function ownKeys(object, enumerableOnly) { var keys = Object.keys(object); if (Object.getOwnPropertySymbols) { var symbols = Object.getOwnPropertySymbols(object); if (enumerableOnly) symbols = symbols.filter(function (sym) { return Object.getOwnPropertyDescriptor(object, sym).enumerable; }); keys.push.apply(keys, symbols); } return keys; }

function _objectSpread(target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i] != null ? arguments[i] : {}; if (i % 2) { ownKeys(Object(source), true).forEach(function (key) { _defineProperty(target, key, source[key]); }); } else if (Object.getOwnPropertyDescriptors) { Object.defineProperties(target, Object.getOwnPropertyDescriptors(source)); } else { ownKeys(Object(source)).forEach(function (key) { Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key)); }); } } return target; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _iterableToArrayLimit(arr, i) { if (typeof Symbol === "undefined" || !(Symbol.iterator in Object(arr))) return; var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

var areAllCheckboxesChecked = function areAllCheckboxesChecked(checkboxes) {
  return Object.values(checkboxes).every(function (checked) {
    return checked;
  });
};

var mapCheckboxesToAccordionSections = function mapCheckboxesToAccordionSections(sections, checkboxes, selectSectionLabel, createFieldName, CheckboxComponent, FieldComponent, sectionCheckboxStates, toggleSectionCheckboxes) {
  return sections.map(function (section) {
    return {
      id: section.value,
      title: section.label,
      content: /*#__PURE__*/_react["default"].createElement(_Grid["default"], {
        container: true
      }, checkboxes[section.value].map(function (checkbox) {
        return /*#__PURE__*/_react["default"].createElement(_Grid["default"], {
          key: "".concat(section.value, ".").concat(checkbox.value),
          item: true,
          xs: 6,
          md: 4
        }, /*#__PURE__*/_react["default"].createElement(FieldComponent, {
          component: CheckboxComponent,
          name: createFieldName(section.value, checkbox.value),
          checkboxLabel: checkbox.label
        }));
      })),
      rightAlignedContent: /*#__PURE__*/_react["default"].createElement(CheckboxComponent, {
        value: sectionCheckboxStates[section.value] || false,
        onChange: toggleSectionCheckboxes(section.value),
        checkboxLabel: selectSectionLabel || '',
        labelOnLeftSide: true
      })
    };
  });
};

var CheckboxAccordion = function CheckboxAccordion(props) {
  var _useState = (0, _react.useState)({}),
      _useState2 = _slicedToArray(_useState, 2),
      sectionCheckboxStates = _useState2[0],
      setSectionCheckboxStates = _useState2[1];

  var updateSectionCheckboxValue = function updateSectionCheckboxValue(section, checkboxValue) {
    return setSectionCheckboxStates(_objectSpread(_objectSpread({}, sectionCheckboxStates), {}, _defineProperty({}, section, checkboxValue)));
  };

  var _useFormikContext = (0, _formik.useFormikContext)(),
      values = _useFormikContext.values,
      setFieldValue = _useFormikContext.setFieldValue;

  var createFieldName = function createFieldName(sectionName, checkboxFieldName) {
    return "".concat(props.fieldGroupName, ".").concat(sectionName, ".").concat(checkboxFieldName);
  };

  var setCheckboxFieldValuesForSection = function setCheckboxFieldValuesForSection(sectionCheckboxValues, sectionName, checkboxValue) {
    return Object.keys(sectionCheckboxValues).forEach(function (checkboxFieldName) {
      return setFieldValue(createFieldName(sectionName, checkboxFieldName), checkboxValue);
    });
  };

  var checkboxFieldValues = values[props.fieldGroupName];

  var toggleSelectAll = function toggleSelectAll(checkboxValue) {
    Object.keys(checkboxFieldValues).forEach(function (sectionName) {
      var sectionCheckboxValues = checkboxFieldValues[sectionName];
      setCheckboxFieldValuesForSection(sectionCheckboxValues, sectionName, checkboxValue);
    });
  };

  var toggleSectionCheckboxes = function toggleSectionCheckboxes(sectionName) {
    return function (checkboxValue) {
      updateSectionCheckboxValue(sectionName, checkboxValue);
      var sectionCheckboxValues = checkboxFieldValues[sectionName];
      setCheckboxFieldValuesForSection(sectionCheckboxValues, sectionName, checkboxValue);
    };
  };

  (0, _react.useEffect)(function () {
    var updatedSectionValues = {};
    Object.keys(checkboxFieldValues).forEach(function (sectionName) {
      var sectionCheckboxValues = checkboxFieldValues[sectionName];
      var allCheckboxesInSectionChecked = areAllCheckboxesChecked(sectionCheckboxValues);
      updatedSectionValues[sectionName] = allCheckboxesInSectionChecked;
    });
    setSectionCheckboxStates(updatedSectionValues);
  }, [checkboxFieldValues]);
  var FieldComponent = props.fieldComponent;
  var ButtonComponent = props.buttonComponent;
  var CheckboxComponent = props.checkboxComponent;
  return /*#__PURE__*/_react["default"].createElement(_react["default"].Fragment, null, !!props.sections.length && /*#__PURE__*/_react["default"].createElement(_ButtonPanel["default"], null, /*#__PURE__*/_react["default"].createElement(ButtonComponent, {
    onClick: function onClick() {
      return toggleSelectAll(true);
    }
  }, (0, _i18n.translate)(_i18n.keys.selectAll)), /*#__PURE__*/_react["default"].createElement(ButtonComponent, {
    onClick: function onClick() {
      return toggleSelectAll(false);
    }
  }, (0, _i18n.translate)(_i18n.keys.deselectAll))), /*#__PURE__*/_react["default"].createElement(_Accordion["default"], {
    sections: mapCheckboxesToAccordionSections(props.sections, props.checkboxes, props.selectSectionLabel, createFieldName, CheckboxComponent, FieldComponent, sectionCheckboxStates, toggleSectionCheckboxes)
  }));
};

CheckboxAccordion.propTypes = {
  fieldGroupName: _propTypes["default"].string.isRequired,
  fieldComponent: _propTypes["default"].func.isRequired,
  buttonComponent: _propTypes["default"].func,
  checkboxComponent: _propTypes["default"].func,
  selectSectionLabel: _propTypes["default"].string,
  sections: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    label: _propTypes["default"].string.isRequired,
    value: _propTypes["default"].string.isRequired
  })),
  checkboxes: _propTypes["default"].objectOf(_propTypes["default"].arrayOf(_propTypes["default"].shape({
    label: _propTypes["default"].string.isRequired,
    value: _propTypes["default"].string.isRequired
  })))
};
CheckboxAccordion.defaultProps = {
  buttonComponent: _Button["default"],
  checkboxComponent: _Checkbox["default"]
};
var _default = CheckboxAccordion;
exports["default"] = _default;