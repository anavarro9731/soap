"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.useSubscribeToApiEvent = void 0;

var _react = require("react");

var _postal = _interopRequireDefault(require("postal"));

var _soap = require("../soap");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var useSubscribeToApiEvent = function useSubscribeToApiEvent(eventName, onEventReceived) {
  var channel = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : _soap.bus.channels.events;
  (0, _react.useEffect)(function () {
    var sub = _soap.bus.subscribe(channel, eventName, onEventReceived);

    return function () {
      _postal["default"].unsubscribe(sub);
    };
  }, []);
};

exports.useSubscribeToApiEvent = useSubscribeToApiEvent;