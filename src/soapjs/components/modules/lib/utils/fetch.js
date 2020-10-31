"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.fetch = void 0;

var fetch = function fetch(endpoint, callback) {
  try {
    global.fetch(endpoint).then(function (response) {
      return response.json();
    }).then(function (result) {
      return callback(result);
    });
  } catch (e) {
    console.log(e);
  }
};

exports.fetch = fetch;