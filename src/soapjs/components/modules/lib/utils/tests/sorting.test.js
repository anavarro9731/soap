"use strict";

var _sorting = require("../sorting");

describe('when calling sortArrayByObjectPropertyAlphanumerically', function () {
  var array, sortedArray, expectedArray;
  beforeEach(function arrange() {
    array = [{
      name: 'b'
    }, {
      name: '1'
    }, {
      name: 'c2'
    }, {
      name: '34'
    }, {
      name: '9c'
    }];
    expectedArray = [{
      name: '1'
    }, {
      name: '9c'
    }, {
      name: '34'
    }, {
      name: 'b'
    }, {
      name: 'c2'
    }];
  });
  beforeEach(function act() {
    sortedArray = (0, _sorting.sortArrayByObjectPropertyAlphanumerically)(array, function (array) {
      return array.name;
    });
  });
  it('should correctly order the array alphanumerically', function () {
    expect(sortedArray).toEqual(expectedArray);
  });
});