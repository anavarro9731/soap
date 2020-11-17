import { registerMessageTypes } from "./messages";
import { fetch } from "../utils/fetch";






export default {
  log(msg) {
    console.log(msg);
  },
  setup(sender) {
    const fr = process.env.FunctionAppRoot;
    const sr = process.env.ServiceBusRoot;
    const qn = process.env.ServiceBusQueue;
    this.sender = msg => sender(msg, sr, qn);
    
    //* register messages
    fetch(`${fr}/GetJsonSchema`, (jsonArrayOfMessages)=> {
      registerMessageTypes(jsonArrayOfMessages);
    });
    
  },
  send(message) {
    sender(message);
  }
};
