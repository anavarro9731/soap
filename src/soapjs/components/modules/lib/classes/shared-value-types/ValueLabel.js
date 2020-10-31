"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ValueLabel = void 0;

var _soap = require("../../soap");

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var ValueLabel = function ValueLabel(_ref) {
  var label = _ref.label,
      value = _ref.value;

  _classCallCheck(this, ValueLabel);

  (0, _soap.validateArgs)([{
    label: label
  }, _soap.types.string], [{
    value: value
  }, _soap.types.string]);
  this.label = label;
  this.value = value;
};

exports.ValueLabel = ValueLabel;