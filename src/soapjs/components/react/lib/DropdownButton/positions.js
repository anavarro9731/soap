"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.isPositionLeft = exports.POSITIONS = void 0;
var POSITIONS = {
  RIGHT: 'right',
  LEFT: 'left'
};
exports.POSITIONS = POSITIONS;

var isPositionLeft = function isPositionLeft(position) {
  return position === POSITIONS.LEFT;
};

exports.isPositionLeft = isPositionLeft;