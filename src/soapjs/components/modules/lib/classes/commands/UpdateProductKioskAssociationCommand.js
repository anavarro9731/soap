"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.UpdateProductKioskAssociationCommand_c108v1 = void 0;

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

var UpdateProductKioskAssociationCommand_c108v1 = /*#__PURE__*/function (_ApiCommand) {
  _inherits(UpdateProductKioskAssociationCommand_c108v1, _ApiCommand);

  var _super = _createSuper(UpdateProductKioskAssociationCommand_c108v1);

  function UpdateProductKioskAssociationCommand_c108v1(_ref) {
    var _this;

    var c108_productId = _ref.c108_productId,
        c108_applicableKioskIds = _ref.c108_applicableKioskIds;

    _classCallCheck(this, UpdateProductKioskAssociationCommand_c108v1);

    (0, _soap.validateArgs)([{
      c108_productId: c108_productId
    }, _soap.types.string], [{
      c108_applicableKioskIds: c108_applicableKioskIds
    }, [_soap.types.string]]);
    _this = _super.call(this);
    _this.schema = 'UpdateProductKioskAssociationCommand_c108v1';
    _this.c108_productId = c108_productId;
    _this.c108_applicableKioskIds = c108_applicableKioskIds;
    return _this;
  }

  return UpdateProductKioskAssociationCommand_c108v1;
}(_soap.ApiCommand);

exports.UpdateProductKioskAssociationCommand_c108v1 = UpdateProductKioskAssociationCommand_c108v1;