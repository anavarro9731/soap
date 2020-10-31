"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var _util = require("./util.js");

var _postal = _interopRequireDefault(require("postal"));

var _config = _interopRequireDefault(require("./config"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var _default = {
  publish: function publish(channel, schema, data, conversationId) {
    (0, _util.validateArgs)([{
      channel: channel
    }, _util.types.string], [{
      schema: schema
    }, _util.types.string], [{
      data: data
    }, _util.types.object], [{
      conversationId: conversationId
    }, _util.types.string, _util.optional]);
    var topic = schema;
    if (!!conversationId) topic += ".".concat(conversationId);

    _config["default"].log("PUBLISHING ".concat(JSON.stringify(data), " to channel: ").concat(channel, " topic: ").concat(topic));

    _postal["default"].publish({
      channel: channel,
      topic: topic,
      data: data
    });
  },
  subscribe: function subscribe(channel, schema, callback, conversationId) {
    (0, _util.validateArgs)([{
      channel: channel
    }, _util.types.string], [{
      schema: schema
    }, _util.types.string], [{
      callback: callback
    }, _util.types["function"]], [{
      conversationId: conversationId
    }, _util.types.string, _util.optional]);
    var topic = schema;
    if (!!conversationId) topic += ".".concat(conversationId);

    var sub = _postal["default"].subscribe({
      channel: channel,
      topic: topic,
      callback: callback
    });

    _config["default"].log("SUBSCRIBED to channel: ".concat(channel, " topic: ").concat(topic));

    return sub;
  },
  closeConversation: function closeConversation(conversationId) {
    (0, _util.validateArgs)([{
      conversationId: conversationId
    }, _util.types.string]);

    _postal["default"].unsubscribeFor(function (s) {
      return s.topic === "*.".concat(conversationId);
    });

    _config["default"].log("UNSUBSCRIBED to all messages in conversation: ".concat(conversationId));
  },
  channels: {
    queries: 'queries',
    events: 'events',
    commands: 'commands',
    errors: 'errors'
  }
};
exports["default"] = _default;