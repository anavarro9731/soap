"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.displayToast = exports.TOAST_TYPES = void 0;

var _reactToastify = require("react-toastify");

var TOAST_TYPES = {
  ERROR: 'error',
  SUCCESS: 'success',
  WARNING: 'warning'
};
exports.TOAST_TYPES = TOAST_TYPES;

var displayToast = function displayToast() {
  var toastType = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : TOAST_TYPES.SUCCESS;
  var message = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : '';
  var position = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : _reactToastify.toast.POSITION.TOP_RIGHT;
  return _reactToastify.toast[toastType](message, {
    position: position
  });
};

exports.displayToast = displayToast;