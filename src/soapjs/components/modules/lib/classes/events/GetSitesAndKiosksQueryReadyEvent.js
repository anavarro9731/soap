"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.GetSitesAndKiosksQueryReadyEvent_e118v1 = void 0;

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

var GetSitesAndKiosksQueryReadyEvent_e118v1 = /*#__PURE__*/function (_ApiEvent) {
  _inherits(GetSitesAndKiosksQueryReadyEvent_e118v1, _ApiEvent);

  var _super = _createSuper(GetSitesAndKiosksQueryReadyEvent_e118v1);

  function GetSitesAndKiosksQueryReadyEvent_e118v1(_ref) {
    var _this;

    var e118_sites = _ref.e118_sites;

    _classCallCheck(this, GetSitesAndKiosksQueryReadyEvent_e118v1);

    (0, _soap.validateArgs)([{
      e118_sites: e118_sites
    }, [Site]]);
    _this = _super.call(this);
    _this.schema = 'GetSitesAndKiosksQueryReadyEvent_e118v1';
    _this.e118_sites = e118_sites;
    return _this;
  }

  return GetSitesAndKiosksQueryReadyEvent_e118v1;
}(_soap.ApiEvent);

exports.GetSitesAndKiosksQueryReadyEvent_e118v1 = GetSitesAndKiosksQueryReadyEvent_e118v1;

var Site = function Site(_ref2) {
  var e118_siteId = _ref2.e118_siteId,
      e118_siteName = _ref2.e118_siteName,
      e118_kiosks = _ref2.e118_kiosks;

  _classCallCheck(this, Site);

  (0, _soap.validateArgs)([{
    e118_siteId: e118_siteId
  }, _soap.types.string], [{
    e118_siteName: e118_siteName
  }, _soap.types.string], [{
    e118_kiosks: e118_kiosks
  }, [Kiosk]]);
  this.e118_siteId = e118_siteId;
  this.e118_siteName = e118_siteName;
  this.e118_kiosks = e118_kiosks;
};

var Kiosk = function Kiosk(_ref3) {
  var e118_kioskId = _ref3.e118_kioskId,
      e118_kioskName = _ref3.e118_kioskName;

  _classCallCheck(this, Kiosk);

  (0, _soap.validateArgs)([{
    e118_kioskId: e118_kioskId
  }, _soap.types.string], [{
    e118_kioskName: e118_kioskName
  }, _soap.types.string]);
  this.e118_kioskId = e118_kioskId;
  this.e118_kioskName = e118_kioskName;
};

GetSitesAndKiosksQueryReadyEvent_e118v1.Site = Site;
GetSitesAndKiosksQueryReadyEvent_e118v1.Site.Kiosk = Kiosk;