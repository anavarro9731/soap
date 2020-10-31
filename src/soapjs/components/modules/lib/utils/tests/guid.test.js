"use strict";

var _guid = require("../guid");

var crypto = require('crypto');

Object.defineProperty(global.self, 'crypto', {
  value: {
    getRandomValues: function getRandomValues(arr) {
      return crypto.randomBytes(arr.length);
    }
  }
});
describe('when calling generateGuid', function () {
  var generatedGuid, regexTest;
  beforeEach(function arrange() {
    regexTest = RegExp('[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}');
  });
  beforeEach(function act() {
    generatedGuid = (0, _guid.generateGuid)();
  });
  it('should generate a random GUID in the correct format', function () {
    expect(regexTest.test(generatedGuid)).toEqual(true);
  });
});