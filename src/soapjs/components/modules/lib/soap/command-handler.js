"use strict";

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.mockEvent = mockEvent;
exports["default"] = void 0;

var _lodash = _interopRequireDefault(require("lodash"));

var __ = _interopRequireWildcard(require("./util.js"));

var _index = require("./index");

var _config = _interopRequireDefault(require("./config"));

var _messages = require("./messages.js");

function _getRequireWildcardCache() { if (typeof WeakMap !== "function") return null; var cache = new WeakMap(); _getRequireWildcardCache = function _getRequireWildcardCache() { return cache; }; return cache; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } if (obj === null || _typeof(obj) !== "object" && typeof obj !== "function") { return { "default": obj }; } var cache = _getRequireWildcardCache(); if (cache && cache.has(obj)) { return cache.get(obj); } var newObj = {}; var hasPropertyDescriptor = Object.defineProperty && Object.getOwnPropertyDescriptor; for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = hasPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : null; if (desc && (desc.get || desc.set)) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } newObj["default"] = obj; if (cache) { cache.set(obj, newObj); } return newObj; }

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

var mockEvents = {};

function mockEvent(commandClass, correspondingEvents) {
  correspondingEvents = Array.isArray(correspondingEvents) ? correspondingEvents : [correspondingEvents];
  (0, __.validateArgs)([{
    commandClass: commandClass
  }, __.types["function"]], [{
    correspondingEvents: correspondingEvents
  }, [__.types.object]]);
  var commandName = commandClass.name;
  correspondingEvents.forEach(function (event) {
    var eventEnvelope = new _messages.MessageEnvelope(event, "we won't know till later, so we'll replace it later");
    var commandBusChannel = commandClass.prototype instanceof _messages.ApiQuery ? _index.bus.channels.queries : _index.bus.channels.commands;
    eventEnvelope.headers.channel = commandBusChannel; //- would normally be set with publisher which is out of our control (e.g. server)

    /* normally we would want to set eventEnvelope.headers.queryHash = commandHash;
      but querycache is a singleton in essence and we don't want that shared between tests */

    if (!mockEvents[commandName]) {
      mockEvents[commandName] = [];
    }

    mockEvents[commandName].push(eventEnvelope);
  });
}
/*
top-level function: separate process block, use out vars, validate args
nested functions: separate process block if complex
*/


var _default = {
  handle: function handle(command, onResponse, acceptableStalenessFactorInSeconds) {
    (0, __.validateArgs)([{
      command: command
    }, __.types.object], [{
      onResponse: onResponse
    }, __.types["function"]], [{
      acceptableStalenessFactorInSeconds: acceptableStalenessFactorInSeconds
    }, __.types.number]);
    if (foundCachedResultsWhenDealingWithAQuery(command, onResponse, acceptableStalenessFactorInSeconds)) return;
    var conversationId = (0, __.uuidv4)();
    /* subscribe to response for just a brief moment
    in the event that you send multiple identical requests before the first response is cached
    you will get multiple commands/eventsubscriptions but that should be ok just a little less performant.
    
    also noteworthy is the fact that you can listen for multiple messages if they have different
    schemas  and multiple messages of the same schema until the conversation is ended
    all outbound queries have a conversationid which is terminated when CloseConversation is called
    */

    var commandEnvelope = new _messages.MessageEnvelope(command, conversationId);
    subscribeCallerToEventResponses(commandEnvelope);
    callApiToTriggerEventResponse(commandEnvelope);
    return conversationId;
    /***********************************************************/

    function subscribeCallerToEventResponses(commandEnvelope) {
      //- everything with this conversation id
      _index.bus.subscribe(commandEnvelope.headers.channel, '#', onResponse, commandEnvelope.headers.conversationId);
    }

    function foundCachedResultsWhenDealingWithAQuery(command, onResponse, acceptableStalenessFactorInSeconds) {
      if (command instanceof _messages.ApiQuery) {
        var cacheResult = _index.queryCache.query(command, acceptableStalenessFactorInSeconds);

        _config["default"].log('cache result:', cacheResult);

        if (cacheResult) {
          onResponse(cacheResult.event, undefined);
        }

        return !!cacheResult;
      }
    }

    function callApiToTriggerEventResponse(commandEnvelope) {
      var commandName = commandEnvelope.headers.schema;
      var mockedEventsForCommand = mockEvents[commandName];
      var commandConversationId = commandEnvelope.headers.conversationId;

      if (!!mockedEventsForCommand) {
        attemptToFakeEventResponseFromMockQueue(mockedEventsForCommand, commandConversationId);
      } else {
        _config["default"].getConnected() ? _config["default"].send(commandEnvelope) : _config["default"].addToCommandQueue(commandEnvelope);
      }

      function attemptToFakeEventResponseFromMockQueue(mockedEventsForCommand, commandConversationId) {
        // fake API responses
        mockedEventsForCommand.forEach(function (eventEnvelope) {
          eventEnvelope.headers.conversationId = commandConversationId;

          _index.eventHandler.handle(eventEnvelope);
        });
      }
    }
  }
};
exports["default"] = _default;