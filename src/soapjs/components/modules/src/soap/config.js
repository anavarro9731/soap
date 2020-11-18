let log = { log: console.log };
let sendFunction;

export default {
    set logger(l) {
        log = l;
    },
    get logger() {
        return log;
    },
    set sender(s) {
        sendFunction = s;
    },
    send(message) {
        if (sendFunction === undefined) throw 'sender not defined please set config.sender = (msg) => {};';
        sendFunction(message);
    }
};
