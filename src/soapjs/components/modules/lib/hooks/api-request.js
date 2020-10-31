"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.command = exports.useQuery = void 0;

var _react = require("react");

var _soap = require("../soap");

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _iterableToArrayLimit(arr, i) { if (typeof Symbol === "undefined" || !(Symbol.iterator in Object(arr))) return; var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

var useQuery = function useQuery(query) {
  var _ref = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : {},
      _ref$condition = _ref.condition,
      condition = _ref$condition === void 0 ? true : _ref$condition,
      _ref$refetchOnChange = _ref.refetchOnChange,
      refetchOnChange = _ref$refetchOnChange === void 0 ? [] : _ref$refetchOnChange,
      _ref$acceptableStalen = _ref.acceptableStalenessFactorInSeconds,
      acceptableStalenessFactorInSeconds = _ref$acceptableStalen === void 0 ? 0 : _ref$acceptableStalen;

  var _useState = (0, _react.useState)(),
      _useState2 = _slicedToArray(_useState, 2),
      queryResult = _useState2[0],
      setQueryResult = _useState2[1];

  var refetchOnChangeArray = Array.isArray(refetchOnChange) ? refetchOnChange : [refetchOnChange];

  var onResponse = function onResponse(data) {
    setQueryResult(data);
  };

  (0, _react.useEffect)(function () {
    var conversationId = undefined;

    if (condition === true) {
      conversationId = _soap.commandHandler.handle(query, onResponse, acceptableStalenessFactorInSeconds);
    }

    return function () {
      if (conversationId) {
        _soap.bus.closeConversation(conversationId);
      }
    };
  }, refetchOnChangeArray);
  return queryResult;
};

exports.useQuery = useQuery;

var command = function command(_command) {
  var conversationId = _soap.commandHandler.handle(_command, function () {
    return null;
  }, 0);

  _soap.bus.closeConversation(conversationId);
};

exports.command = command;