"use strict";

var _util = require("../util.js");

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

test("require args test", function () {
  testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], {
    o1: 1,
    o2: "two"
  }); //- fail stringArg

  expect(function () {
    testArgs(null, 1, new Date(), new classOne(), [new classOne(), new classOne()], {
      o1: 1,
      o2: "two"
    });
  }).toThrow(); //- fail numberArg

  expect(function () {
    testArgs("test", null, new Date(), new classOne(), [new classOne(), new classOne()], {
      o1: 1,
      o2: "two"
    });
  }).toThrow(); //- fail dateArg

  expect(function () {
    testArgs("test", 1, null, new classOne(), [new classOne(), new classOne()], {
      o1: 1,
      o2: "two"
    });
  }).toThrow(); //- fail classOneArg

  expect(function () {
    testArgs("test", 1, new Date(), null, [new classOne(), new classOne()], {
      o1: 1,
      o2: "two"
    });
  }).toThrow(); //- fail classOneArrayArg

  expect(function () {
    testArgs("test", 1, new Date(), new classOne(), [new classOne(), null], {
      o1: 1,
      o2: "two"
    });
  }).toThrow(); //- fail optionsArg

  expect(function () {
    testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN); //use NaN cause null is an object in JS
  }).toThrow(); //- fail optional Arg

  expect(function () {
    testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN, null);
  }).toThrow();
});

var classOne = function classOne() {
  _classCallCheck(this, classOne);
};

function testArgs(stringArg, numberArg, dateArg, classOneArg, classOneArrayArg, optionsArg, optionalArg) {
  (0, _util.validateArgs)([{
    stringArg: stringArg
  }, _util.types.string], [{
    numberArg: numberArg
  }, _util.types.number], [{
    dateArg: dateArg
  }, Date], [{
    classOneArg: classOneArg
  }, classOne], [{
    classOneArrayArg: classOneArrayArg
  }, [classOne]], [{
    optionsArg: optionsArg
  }, Object], [{
    optionalArg: optionalArg
  }, _util.types.string, _util.optional]);
}