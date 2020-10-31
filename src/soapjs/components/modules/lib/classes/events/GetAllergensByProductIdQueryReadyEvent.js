"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.GetAllergensByProductIdQueryReadyEvent_e101v1 = void 0;

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

var GetAllergensByProductIdQueryReadyEvent_e101v1 = /*#__PURE__*/function (_ApiEvent) {
  _inherits(GetAllergensByProductIdQueryReadyEvent_e101v1, _ApiEvent);

  var _super = _createSuper(GetAllergensByProductIdQueryReadyEvent_e101v1);

  function GetAllergensByProductIdQueryReadyEvent_e101v1(_ref) {
    var _this;

    var e101_celery = _ref.e101_celery,
        e101_cereal = _ref.e101_cereal,
        e101_crustaceans = _ref.e101_crustaceans,
        e101_eggs = _ref.e101_eggs,
        e101_fish = _ref.e101_fish,
        e101_lupin = _ref.e101_lupin,
        e101_milk = _ref.e101_milk,
        e101_molluscs = _ref.e101_molluscs,
        e101_mustard = _ref.e101_mustard,
        e101_nuts = _ref.e101_nuts,
        e101_peanut = _ref.e101_peanut,
        e101_seasame = _ref.e101_seasame,
        e101_soya = _ref.e101_soya,
        e101_sulphar = _ref.e101_sulphar;

    _classCallCheck(this, GetAllergensByProductIdQueryReadyEvent_e101v1);

    (0, _soap.validateArgs)([{
      e101_celery: e101_celery
    }, _soap.types["boolean"]], [{
      e101_cereal: e101_cereal
    }, _soap.types["boolean"]], [{
      e101_crustaceans: e101_crustaceans
    }, _soap.types["boolean"]], [{
      e101_eggs: e101_eggs
    }, _soap.types["boolean"]], [{
      e101_fish: e101_fish
    }, _soap.types["boolean"]], [{
      e101_lupin: e101_lupin
    }, _soap.types["boolean"]], [{
      e101_milk: e101_milk
    }, _soap.types["boolean"]], [{
      e101_molluscs: e101_molluscs
    }, _soap.types["boolean"]], [{
      e101_mustard: e101_mustard
    }, _soap.types["boolean"]], [{
      e101_nuts: e101_nuts
    }, _soap.types["boolean"]], [{
      e101_peanut: e101_peanut
    }, _soap.types["boolean"]], [{
      e101_seasame: e101_seasame
    }, _soap.types["boolean"]], [{
      e101_soya: e101_soya
    }, _soap.types["boolean"]], [{
      e101_sulphar: e101_sulphar
    }, _soap.types["boolean"]]);
    _this = _super.call(this);
    _this.schema = 'GetAllergensByProductIdQueryReadyEvent_e101v1';
    _this.e101_celery = e101_celery;
    _this.e101_cereal = e101_cereal;
    _this.e101_crustaceans = e101_crustaceans;
    _this.e101_eggs = e101_eggs;
    _this.e101_fish = e101_fish;
    _this.e101_lupin = e101_lupin;
    _this.e101_milk = e101_milk;
    _this.e101_molluscs = e101_molluscs;
    _this.e101_mustard = e101_mustard;
    _this.e101_nuts = e101_nuts;
    _this.e101_peanut = e101_peanut;
    _this.e101_seasame = e101_seasame;
    _this.e101_soya = e101_soya;
    _this.e101_sulphar = e101_sulphar;
    return _this;
  }

  return GetAllergensByProductIdQueryReadyEvent_e101v1;
}(_soap.ApiEvent);

exports.GetAllergensByProductIdQueryReadyEvent_e101v1 = GetAllergensByProductIdQueryReadyEvent_e101v1;