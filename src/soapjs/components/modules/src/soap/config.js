if (
  globalThis.soap_connected === undefined &&
  globalThis.soap_startupCommandQueue === undefined
) {
  globalThis.soap_connected = false;
  globalThis.soap_startupCommandQueue = [];
}

export default {
  log(msg) {
    console.log(msg);
  },
  setConnected(value) {
    globalThis.soap_connected = value;
  },
  addToCommandQueue(command) {
    globalThis.soap_startupCommandQueue.push(command);
  },
  getStartupCommandQueue() {
    return globalThis.soap_startupCommandQueue;
  },
  getConnected() {
    return globalThis.soap_connected;
  },
  setSend(sendFunction) {
    if (sendFunction) globalThis.soap_send = sendFunction;
  },
  send(message) {
    globalThis.soap_send(message);
  },
};
