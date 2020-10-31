"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _lodash = _interopRequireDefault(require("lodash"));

var _util = require("./util.js");

var _messages = require("./messages.js");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var cache = [];
var _default = {
  addOrReplace: function addOrReplace(queryHash, event) {
    (0, _util.validateArgs)([{
      queryHash: queryHash
    }, _util.types.string], [{
      event: event
    }, _messages.ApiEvent]);
    /* concurrency risk, prob not possible with 
        default browser STA, unless interrupted by async etc */

    _lodash["default"].remove(cache, function (i) {
      return i.queryHash === queryHash;
    });

    cache.push({
      queryHash: queryHash,
      event: event,
      timestamp: new Date().getTime()
    });
  },
  query: function query(_query, acceptableStalenessFactorInSeconds) {
    (0, _util.validateArgs)([{
      query: _query
    }, _messages.ApiQuery], [{
      acceptableStalenessFactorInSeconds: acceptableStalenessFactorInSeconds
    }, _util.types.number]);
    var queryHash = (0, _util.md5Hash)(_query);
    var result;

    if (acceptableStalenessFactorInSeconds > 0) {
      result = _lodash["default"].find(cache, function (i) {
        return i.queryHash === queryHash && new Date().getTime() - i.timestamp <= acceptableStalenessFactorInSeconds * 1000;
      }).event;
    }

    return result;
  }
};
exports["default"] = _default;