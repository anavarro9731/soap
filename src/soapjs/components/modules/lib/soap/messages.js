"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.TestEvent = exports.TestEvent_Results = exports.ApiEvent = exports.ApiQuery = exports.ApiCommand = exports.ApiMessage = exports.MessageEnvelope = void 0;

var _util = require("./util.js");

var _index = require("./index");

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

function _createSuper(Derived) { var hasNativeReflectConstruct = _isNativeReflectConstruct(); return function _createSuperInternal() { var Super = _getPrototypeOf(Derived), result; if (hasNativeReflectConstruct) { var NewTarget = _getPrototypeOf(this).constructor; result = Reflect.construct(Super, arguments, NewTarget); } else { result = Super.apply(this, arguments); } return _possibleConstructorReturn(this, result); }; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _isNativeReflectConstruct() { if (typeof Reflect === "undefined" || !Reflect.construct) return false; if (Reflect.construct.sham) return false; if (typeof Proxy === "function") return true; try { Date.prototype.toString.call(Reflect.construct(Date, [], function () {})); return true; } catch (e) { return false; } }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

function _objectWithoutProperties(source, excluded) { if (source == null) return {}; var target = _objectWithoutPropertiesLoose(source, excluded); var key, i; if (Object.getOwnPropertySymbols) { var sourceSymbolKeys = Object.getOwnPropertySymbols(source); for (i = 0; i < sourceSymbolKeys.length; i++) { key = sourceSymbolKeys[i]; if (excluded.indexOf(key) >= 0) continue; if (!Object.prototype.propertyIsEnumerable.call(source, key)) continue; target[key] = source[key]; } } return target; }

function _objectWithoutPropertiesLoose(source, excluded) { if (source == null) return {}; var target = {}; var sourceKeys = Object.keys(source); var key, i; for (i = 0; i < sourceKeys.length; i++) { key = sourceKeys[i]; if (excluded.indexOf(key) >= 0) continue; target[key] = source[key]; } return target; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var MessageEnvelope = function MessageEnvelope(msg, conversationId) {
  _classCallCheck(this, MessageEnvelope);

  (0, _util.validateArgs)([{
    msg: msg
  }, _util.types.object], [{
    conversationId: conversationId
  }, _util.types.string]);

  var schema = msg.schema,
      busChannel = msg.busChannel,
      payload = _objectWithoutProperties(msg, ["schema", "busChannel"]);

  this.payload = payload;
  this.headers = {
    schema: schema,
    channel: busChannel,
    conversationId: conversationId
  };

  if (msg instanceof ApiQuery) {
    this.headers.queryHash = (0, _util.md5Hash)(msg);
  }
};

exports.MessageEnvelope = MessageEnvelope;

var ApiMessage = function ApiMessage() {
  _classCallCheck(this, ApiMessage);
};

exports.ApiMessage = ApiMessage;

var ApiCommand = /*#__PURE__*/function (_ApiMessage) {
  _inherits(ApiCommand, _ApiMessage);

  var _super = _createSuper(ApiCommand);

  function ApiCommand() {
    _classCallCheck(this, ApiCommand);

    return _super.apply(this, arguments);
  }

  _createClass(ApiCommand, [{
    key: "busChannel",
    get: function get() {
      return _index.bus.channels.commands;
    }
  }]);

  return ApiCommand;
}(ApiMessage);

exports.ApiCommand = ApiCommand;

var ApiQuery = /*#__PURE__*/function (_ApiCommand) {
  _inherits(ApiQuery, _ApiCommand);

  var _super2 = _createSuper(ApiQuery);

  function ApiQuery() {
    _classCallCheck(this, ApiQuery);

    return _super2.apply(this, arguments);
  }

  _createClass(ApiQuery, [{
    key: "busChannel",
    get: function get() {
      return _index.bus.channels.queries;
    }
  }]);

  return ApiQuery;
}(ApiCommand);

exports.ApiQuery = ApiQuery;

var ApiEvent = /*#__PURE__*/function (_ApiMessage2) {
  _inherits(ApiEvent, _ApiMessage2);

  var _super3 = _createSuper(ApiEvent);

  function ApiEvent() {
    _classCallCheck(this, ApiEvent);

    return _super3.apply(this, arguments);
  }

  return ApiEvent;
}(ApiMessage);

exports.ApiEvent = ApiEvent;

var TestEvent_Results = function TestEvent_Results(id) {
  _classCallCheck(this, TestEvent_Results);

  this.$type = 'TestEvent_Results';
  this.id = id;
};

exports.TestEvent_Results = TestEvent_Results;

var TestEvent = /*#__PURE__*/function (_ApiEvent) {
  _inherits(TestEvent, _ApiEvent);

  var _super4 = _createSuper(TestEvent);

  function TestEvent(resultIds) {
    var _this;

    _classCallCheck(this, TestEvent);

    _this = _super4.call(this);
    _this.$type = 'TestEvent';
    if (!!resultIds) _this.resultIds = resultIds.map(function (x) {
      return new TestEvent_Results(x);
    });
    return _this;
  }

  return TestEvent;
}(ApiEvent);

exports.TestEvent = TestEvent;