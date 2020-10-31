"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.HtmlBodyAndContentFullHeight = void 0;

var _styledComponents = require("styled-components");

function _templateObject() {
  var data = _taggedTemplateLiteral(["\n\thtml {\n    height: 100%;\n  }\n\n  body {\n    height: 100%;\n    margin: 0;\n  }\n\n  #content {\n    height: 100%;\n  }\n"]);

  _templateObject = function _templateObject() {
    return data;
  };

  return data;
}

function _taggedTemplateLiteral(strings, raw) { if (!raw) { raw = strings.slice(0); } return Object.freeze(Object.defineProperties(strings, { raw: { value: Object.freeze(raw) } })); }

var HtmlBodyAndContentFullHeight = (0, _styledComponents.createGlobalStyle)(_templateObject());
exports.HtmlBodyAndContentFullHeight = HtmlBodyAndContentFullHeight;