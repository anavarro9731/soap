"use strict";

var _index = require("../index.js");

test("unhandled errors are caught", function () {
  //arrange
  (0, _index.wireErrorHandlerOfLastResort)(function (error) {
    //assert
    expect(error.message).toMatch(/Whoa!/g);
  });

  try {
    //act 
    throw new Error("Whoa!");
  } catch (e) {
    window.onerror.call(window, e.toString());
  }
});