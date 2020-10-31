"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

var _queryCache = _interopRequireDefault(require("../query-cache.js"));

var _messages = require("../messages.js");

var __ = _interopRequireWildcard(require("../util.js"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

test("queries filtered results", function () {
  _queryCache["default"].addOrReplace(__.md5Hash(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "2342342342"
  })), Object.assign(new _messages.ApiEvent()));

  _queryCache["default"].addOrReplace(__.md5Hash(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "6788676576576"
  })), Object.assign(new _messages.ApiEvent()));

  _queryCache["default"].addOrReplace(__.md5Hash(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "57567464564"
  })), Object.assign(new _messages.ApiEvent()));

  var result = _queryCache["default"].query(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "2342342342"
  }), 10);

  expect(result).toBeInstanceOf(_messages.ApiEvent);
});
test("queries filtered results", function () {
  _queryCache["default"].addOrReplace(__.md5Hash(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "2342342342"
  })), Object.assign(new _messages.ApiEvent()));

  _queryCache["default"].addOrReplace(__.md5Hash(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "6788676576576"
  })), Object.assign(new _messages.ApiEvent()));

  _queryCache["default"].addOrReplace(__.md5Hash(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "57567464564"
  })), Object.assign(new _messages.ApiEvent()));

  var result = _queryCache["default"].query(Object.assign(new _messages.ApiQuery(), {
    pointlessprop: "6788676576576"
  }), 5);

  expect(result).toBeInstanceOf(_messages.ApiEvent);
});