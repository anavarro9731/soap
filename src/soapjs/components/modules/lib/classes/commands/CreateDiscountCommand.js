"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.CreateDiscountCommand_c101v1 = void 0;

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

var CreateDiscountCommand_c101v1 = /*#__PURE__*/function (_ApiCommand) {
  _inherits(CreateDiscountCommand_c101v1, _ApiCommand);

  var _super = _createSuper(CreateDiscountCommand_c101v1);

  function CreateDiscountCommand_c101v1(_ref) {
    var _this;

    var c101_discountId = _ref.c101_discountId,
        c101_discountName = _ref.c101_discountName,
        c101_discountType = _ref.c101_discountType,
        c101_discountAmount = _ref.c101_discountAmount,
        c101_daysDiscountIsActive = _ref.c101_daysDiscountIsActive,
        c101_minimumSpend = _ref.c101_minimumSpend,
        c101_discountCode = _ref.c101_discountCode,
        c101_includeModifiers = _ref.c101_includeModifiers,
        c101_onSaleNow = _ref.c101_onSaleNow,
        c101_freeModifiers = _ref.c101_freeModifiers,
        c101_startTime = _ref.c101_startTime,
        c101_endTime = _ref.c101_endTime,
        c101_allDay = _ref.c101_allDay;

    _classCallCheck(this, CreateDiscountCommand_c101v1);

    (0, _soap.validateArgs)([{
      c101_discountId: c101_discountId
    }, _soap.types.string], [{
      c101_discountName: c101_discountName
    }, _soap.types.string], [{
      c101_discountType: c101_discountType
    }, _soap.types.string], [{
      c101_discountAmount: c101_discountAmount
    }, _soap.types.number], [{
      c101_daysDiscountIsActive: c101_daysDiscountIsActive
    }, [_soap.types.string]], [{
      c101_minimumSpend: c101_minimumSpend
    }, _soap.types.number], [{
      c101_discountCode: c101_discountCode
    }, _soap.types.string], [{
      c101_includeModifiers: c101_includeModifiers
    }, _soap.types["boolean"]], [{
      c101_onSaleNow: c101_onSaleNow
    }, _soap.types["boolean"]], [{
      c101_freeModifiers: c101_freeModifiers
    }, _soap.types["boolean"]], [{
      c101_startTime: c101_startTime
    }, _soap.types.string, _soap.optional], [{
      c101_endTime: c101_endTime
    }, _soap.types.string, _soap.optional], [{
      c101_allDay: c101_allDay
    }, _soap.types["boolean"]]);
    _this = _super.call(this);
    _this.schema = 'CreateDiscountCommand_c101v1';
    _this.c101_discountId = c101_discountId;
    _this.c101_discountName = c101_discountName;
    _this.c101_discountType = c101_discountType;
    _this.c101_discountAmount = c101_discountAmount;
    _this.c101_daysDiscountIsActive = c101_daysDiscountIsActive;
    _this.c101_minimumSpend = c101_minimumSpend;
    _this.c101_discountCode = c101_discountCode;
    _this.c101_includeModifiers = c101_includeModifiers;
    _this.c101_onSaleNow = c101_onSaleNow;
    _this.c101_freeModifiers = c101_freeModifiers;
    _this.c101_startTime = c101_startTime;
    _this.c101_endTime = c101_endTime;
    _this.c101_allDay = c101_allDay;
    return _this;
  }

  return CreateDiscountCommand_c101v1;
}(_soap.ApiCommand);

exports.CreateDiscountCommand_c101v1 = CreateDiscountCommand_c101v1;