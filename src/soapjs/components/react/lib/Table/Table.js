"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _react = _interopRequireDefault(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var S = _interopRequireWildcard(require("./style"));

var _defaults = require("../../modules/style/defaults");

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var mapColumnsToHeaders = function mapColumnsToHeaders(props) {
  return props.columns.map(function (column) {
    return /*#__PURE__*/_react["default"].createElement(S.Cell, {
      key: column.columnId,
      textColour: props.headerTextColour,
      borderColour: props.tableBorderColour,
      backgroundColour: props.headerBackgroundColour,
      fontWeight: "bold"
    }, /*#__PURE__*/_react["default"].createElement(S.CellContent, {
      padding: props.headerCellPadding
    }, column.header));
  });
};

var mapDataToRows = function mapDataToRows(props) {
  return props.data.map(function (dataRow) {
    return props.columns.map(function (column) {
      return /*#__PURE__*/_react["default"].createElement(S.Cell, {
        key: column.columnId,
        textColour: props.tableTextColour,
        borderColour: props.tableBorderColour,
        backgroundColour: props.tableBackgroundColour
      }, /*#__PURE__*/_react["default"].createElement(S.CellContent, {
        padding: props.cellPadding
      }, column.render(dataRow)));
    });
  });
};

var Table = function Table(props) {
  var headers = mapColumnsToHeaders(props);
  var rows = mapDataToRows(props);
  var columnWidths = props.columns.reduce(function (widths, column) {
    return "".concat(widths, " ").concat(column.width || '1fr');
  }, '');
  return /*#__PURE__*/_react["default"].createElement(S.Table, {
    columnWidths: columnWidths
  }, headers, rows);
};

Table.propTypes = {
  data: _propTypes["default"].array,
  columns: _propTypes["default"].arrayOf(_propTypes["default"].shape({
    columnId: _propTypes["default"].string.isRequired,
    header: _propTypes["default"].string,
    render: _propTypes["default"].func,
    width: _propTypes["default"].string
  })).isRequired,
  headerTextColour: _propTypes["default"].string,
  headerBackgroundColour: _propTypes["default"].string,
  tableTextColour: _propTypes["default"].string,
  tableBackgroundColour: _propTypes["default"].string,
  tableBorderColour: _propTypes["default"].string,
  headerCellPadding: _propTypes["default"].string,
  cellPadding: _propTypes["default"].string
};
Table.defaultProps = {
  headerTextColour: _defaults.defaultTextColour,
  headerBackgroundColour: _defaults.defaultLightBackgroundColour,
  tableTextColour: _defaults.defaultTextColour,
  tableBackgroundColour: _defaults.defaultLightBackgroundColour,
  tableBorderColour: _defaults.defaultBorderColour,
  headerCellPadding: '10px',
  cellPadding: '8px'
};
var _default = Table;
exports["default"] = _default;