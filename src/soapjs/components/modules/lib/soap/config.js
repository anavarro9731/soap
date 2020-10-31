"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var M = _interopRequireWildcard(require("./messages"));

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

if (globalThis.soap_connected === undefined && globalThis.soap_startupCommandQueue === undefined) {
  globalThis.soap_connected = false;
  globalThis.soap_startupCommandQueue = [];
}

var _default = {
  log: function log(msg) {
    console.log(msg);
  },
  getMessages: function getMessages() {
    return M;
  },
  setConnected: function setConnected(value) {
    globalThis.soap_connected = value;
  },
  addToCommandQueue: function addToCommandQueue(command) {
    globalThis.soap_startupCommandQueue.push(command);
  },
  getStartupCommandQueue: function getStartupCommandQueue() {
    return globalThis.soap_startupCommandQueue;
  },
  getConnected: function getConnected() {
    return globalThis.soap_connected;
  },
  setSend: function setSend(sendFunction) {
    if (sendFunction) globalThis.soap_send = sendFunction;
  },
  send: function send(message) {
    globalThis.soap_send(message);
  }
};
exports["default"] = _default;