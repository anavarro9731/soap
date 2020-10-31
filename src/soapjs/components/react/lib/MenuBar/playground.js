"use strict";

var _react = _interopRequireDefault(require("react"));

var _reactDom = _interopRequireDefault(require("react-dom"));

var _MenuBar = _interopRequireDefault(require("./MenuBar"));

var _kurveSmile = _interopRequireDefault(require("../../modules/style/images/kurve-smile.png"));

var _AppWrapper = _interopRequireDefault(require("../AppWrapper"));

var _styledComponents = _interopRequireDefault(require("styled-components"));

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

function _templateObject2() {
  var data = _taggedTemplateLiteral(["\n  background-image: url(", ");\n  background-repeat: no-repeat;\n  background-position: center;\n  height: 100%;\n"]);

  _templateObject2 = function _templateObject2() {
    return data;
  };

  return data;
}

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n  height: 100%;\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var BrandImageLink = _styledComponents["default"].a(_templateObject());

var BrandImage = _styledComponents["default"].div(_templateObject2(), function (props) {
  return props.imageUrl;
});

var brandComponent = /*#__PURE__*/_react["default"].createElement(BrandImageLink, {
  href: "/"
}, /*#__PURE__*/_react["default"].createElement(BrandImage, {
  imageUrl: _kurveSmile["default"]
}));

_reactDom["default"].render( /*#__PURE__*/_react["default"].createElement(_AppWrapper["default"], null, /*#__PURE__*/_react["default"].createElement("div", {
  style: {
    height: '50px'
  }
}, /*#__PURE__*/_react["default"].createElement(_MenuBar["default"], {
  brandComponent: brandComponent,
  menuItems: [{
    menuItemId: 'importFile',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Import File")
  }, {
    menuItemId: 'importProducts',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Import Products")
  }, {
    menuItemId: 'configValidation',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Config Validation")
  }, {
    menuItemId: 'syncHistory',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Sync History")
  }, {
    menuItemId: 'editProducts',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Edit Products")
  }],
  rightAlignedItems: [{
    menuItemId: 'heldProducts',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Held Products")
  }, {
    menuItemId: 'archivedProducts',
    component: /*#__PURE__*/_react["default"].createElement("a", {
      style: {
        color: 'white'
      },
      href: ""
    }, "Archived Products")
  }]
}))), document.getElementById('content'));