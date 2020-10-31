"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.AddKioskToSiteCommand_c100v1 = void 0;

var _soap = require("../../soap");

function _typeof(obj) { "@babel/helpers - typeof"; if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

function _createSuper(Derived) { var hasNativeReflectConstruct = _isNativeReflectConstruct(); return function _createSuperInternal() { var Super = _getPrototypeOf(Derived), result; if (hasNativeReflectConstruct) { var NewTarget = _getPrototypeOf(this).constructor; result = Reflect.construct(Super, arguments, NewTarget); } else { result = Super.apply(this, arguments); } return _possibleConstructorReturn(this, result); }; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _isNativeReflectConstruct() { if (typeof Reflect === "undefined" || !Reflect.construct) return false; if (Reflect.construct.sham) return false; if (typeof Proxy === "function") return true; try { Date.prototype.toString.call(Reflect.construct(Date, [], function () {})); return true; } catch (e) { return false; } }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

var AddKioskToSiteCommand_c100v1 = /*#__PURE__*/function (_ApiCommand) {
  _inherits(AddKioskToSiteCommand_c100v1, _ApiCommand);

  var _super = _createSuper(AddKioskToSiteCommand_c100v1);

  function AddKioskToSiteCommand_c100v1(_ref) {
    var _this;

    var c100_siteId = _ref.c100_siteId,
        c100_kiosk = _ref.c100_kiosk;

    _classCallCheck(this, AddKioskToSiteCommand_c100v1);

    (0, _soap.validateArgs)([{
      c100_siteId: c100_siteId
    }, _soap.types.string], [{
      c100_kiosk: c100_kiosk
    }, Kiosk]);
    _this = _super.call(this);
    _this.schema = 'AddKioskToSiteCommand_c100v1';
    _this.c100_siteId = c100_siteId;
    _this.c100_kiosk = c100_kiosk;
    return _this;
  }

  return AddKioskToSiteCommand_c100v1;
}(_soap.ApiCommand);

exports.AddKioskToSiteCommand_c100v1 = AddKioskToSiteCommand_c100v1;

var Kiosk = function Kiosk(_ref2) {
  var c100_kioskId = _ref2.c100_kioskId,
      c100_kioskName = _ref2.c100_kioskName,
      c100_ip = _ref2.c100_ip,
      c100_ip2 = _ref2.c100_ip2,
      c100_buildType = _ref2.c100_buildType,
      c100_buildVersion = _ref2.c100_buildVersion,
      c100_buildName = _ref2.c100_buildName,
      c100_cloudPrinterId = _ref2.c100_cloudPrinterId,
      c100_paymentTerminalId = _ref2.c100_paymentTerminalId,
      c100_kioskType = _ref2.c100_kioskType,
      c100_tableNumbers = _ref2.c100_tableNumbers,
      c100_approvedInstances = _ref2.c100_approvedInstances;

  _classCallCheck(this, Kiosk);

  (0, _soap.validateArgs)([{
    c100_kioskId: c100_kioskId
  }, _soap.types.string], [{
    c100_kioskName: c100_kioskName
  }, _soap.types.string], [{
    c100_ip: c100_ip
  }, _soap.types.string], [{
    c100_ip2: c100_ip2
  }, _soap.types.string], [{
    c100_buildType: c100_buildType
  }, _soap.types.string], [{
    c100_buildVersion: c100_buildVersion
  }, _soap.types.string], [{
    c100_buildName: c100_buildName
  }, _soap.types.string], [{
    c100_cloudPrinterId: c100_cloudPrinterId
  }, _soap.types.string], [{
    c100_paymentTerminalId: c100_paymentTerminalId
  }, _soap.types.string], [{
    c100_kioskType: c100_kioskType
  }, _soap.types.string], [{
    c100_tableNumbers: c100_tableNumbers
  }, [_soap.types.number]], [{
    c100_approvedInstances: c100_approvedInstances
  }, [KioskInstance]]);
  this.c100_kioskId = c100_kioskId;
  this.c100_kioskName = c100_kioskName;
  this.c100_ip = c100_ip;
  this.c100_ip2 = c100_ip2;
  this.c100_buildType = c100_buildType;
  this.c100_buildVersion = c100_buildVersion;
  this.c100_buildName = c100_buildName;
  this.c100_cloudPrinterId = c100_cloudPrinterId;
  this.c100_paymentTerminalId = c100_paymentTerminalId;
  this.c100_kioskType = c100_kioskType;
  this.c100_tableNumbers = c100_tableNumbers;
  this.c100_approvedInstances = c100_approvedInstances;
};

var KioskInstance = function KioskInstance(_ref3) {
  var c100_appInstanceId = _ref3.c100_appInstanceId,
      c100_deviceId = _ref3.c100_deviceId;

  _classCallCheck(this, KioskInstance);

  (0, _soap.validateArgs)([{
    c100_appInstanceId: c100_appInstanceId
  }, _soap.types.string], [{
    c100_deviceId: c100_deviceId
  }, _soap.types.string]);
  this.c100_appInstanceId = c100_appInstanceId;
  this.c100_deviceId = c100_deviceId;
};

AddKioskToSiteCommand_c100v1.Kiosk = Kiosk;
AddKioskToSiteCommand_c100v1.Kiosk.KioskInstance = KioskInstance;