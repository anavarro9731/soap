"use strict";

var _commandHandler = require("../command-handler.js");

var _messages = require("../messages.js");

var _index = require("../index.js");

var _bus = _interopRequireDefault(require("../bus.js"));

var _postal = _interopRequireDefault(require("postal"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

test('queries receive results', function () {
  //- arrange
  var query = Object.assign(new _messages.ApiQuery(), {
    pointlessprop: '12345'
  });
  var gotIt = false;
  (0, _commandHandler.mockEvent)(query, [new _messages.TestEvent([1, 2])]); //- listen for response to query

  var conversationId = _index.commandHandler.handle(query, function (result, postalEnvelope) {
    expect(result instanceof _messages.TestEvent).toBe(true);
    expect(result.resultIds[0] instanceof _messages.TestEvent_Results).toBe(true);

    if (result.resultIds[0].id === 1) {
      gotIt = true;
    }

    var x = result.notexist; //result.notexist = 1;
  }, 0);

  expect(_postal["default"].subscriptions.queries["*.".concat(conversationId)].length).toBe(1);

  _bus["default"].closeConversation(conversationId);

  expect(_postal["default"].subscriptions).toStrictEqual({});
  expect(gotIt).toBe(true);
  expect(_typeof(conversationId)).toBe('string');
});
test('straight commands can receive results', function () {
  //- arrange
  var command = Object.assign(new _messages.ApiCommand(), {
    pointlessprop: '12345'
  });
  var gotIt = false;
  (0, _commandHandler.mockEvent)(command, [new _messages.TestEvent([1, 2])]); //- listen for response to query

  var conversationId = _index.commandHandler.handle(command, function (result, postalEnvelope) {
    if (result.resultIds[0].id === 1) {
      gotIt = true;
    }
  }, 0);

  expect(gotIt).toBe(true);
  expect(_typeof(conversationId)).toBe('string');
});