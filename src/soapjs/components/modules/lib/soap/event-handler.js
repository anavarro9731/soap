"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports["default"] = void 0;

var __ = _interopRequireWildcard(require("./util.js"));

var _index = require("./index.js");

var _config = _interopRequireDefault(require("./config"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _objectWithoutProperties(source, excluded) { if (source == null) return {}; var target = _objectWithoutPropertiesLoose(source, excluded); var key, i; if (Object.getOwnPropertySymbols) { var sourceSymbolKeys = Object.getOwnPropertySymbols(source); for (i = 0; i < sourceSymbolKeys.length; i++) { key = sourceSymbolKeys[i]; if (excluded.indexOf(key) >= 0) continue; if (!Object.prototype.propertyIsEnumerable.call(source, key)) continue; target[key] = source[key]; } } return target; }

function _objectWithoutPropertiesLoose(source, excluded) { if (source == null) return {}; var target = {}; var sourceKeys = Object.keys(source); var key, i; for (i = 0; i < sourceKeys.length; i++) { key = sourceKeys[i]; if (excluded.indexOf(key) >= 0) continue; target[key] = source[key]; } return target; }

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

var _default = {
  handle: function handle(eventEnvelope) {
    (0, __.validateArgs)([{
      eventEnvelope: eventEnvelope
    }, __.types.object]);

    if (eventEnvelope.headers.channel === _index.bus.channels.queries) {
      if (!!eventEnvelope.headers.queryHash) //- then nothing will be cached so you will reask every time (e.g. testing)
        _index.queryCache.addOrReplace(eventEnvelope.headers.queryHash, eventEnvelope.payload);
    }

    var converted = undefined;

    try {
      converted = wrapInProxy(parse(JSON.stringify((0, __.recursivelyConvertObjectNullPropertiesToUndefined)(eventEnvelope.payload))));
      console.log('CONVERTED - ', converted);
    } catch (error) {
      console.error('!!CONVERSION ERROR!! ', error);
    }

    _index.bus.publish(eventEnvelope.headers.channel, eventEnvelope.headers.schema, converted, eventEnvelope.headers.conversationId);

    function wrapInProxy(data) {
      return new Proxy(data, {
        set: function set(target, property, value) {
          // First give the target a chance to handle it
          if (Object.keys(target).indexOf(property) !== -1) {
            return target[property];
          }

          console.error("MISSING PROPERTY EXCEPTION: Attempted to write to ".concat(_typeof(target), ".").concat(property.toString(), " but it does not exist"));
        },
        get: function get(target, property) {
          // First give the target a chance to handle it
          if (Object.keys(target).indexOf(property) !== -1 && property !== 'toJSON') {
            return target[property];
          } else if (property !== 'toJSON') console.error("MISSING PROPERTY EXCEPTION: Attempted to read from ".concat(_typeof(target), ".").concat(property.toString(), " but it does not exist"));
        }
      });
    }

    function parse(data) {
      return JSON.parse(data, function (key, value) {
        if (isObjectLike(value) && value.hasOwnProperty('$type')) {
          if (value instanceof Array) {
            return value.map(function (element, index) {
              return make(element);
            });
          }

          return make(value);
        }

        return value;
      });

      function isObjectLike(value) {
        return _typeof(value) === 'object' && value !== null;
      }

      function make(payloadPortion) {
        var $type = payloadPortion.$type,
            values = _objectWithoutProperties(payloadPortion, ["$type"]);

        var classTypeRegexMatches = $type.match(/\.([^.]+),/i);
        var classType = classTypeRegexMatches[1].replace(/\+/g, '.');
        var newFunction = new Function('messages', 'values', "return new messages.".concat(classType, "(values)"));
        return newFunction(_config["default"].getMessages(), values);
      }
    }
  }
};
exports["default"] = _default;