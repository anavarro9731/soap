import {types, validateArgs, md5Hash, optional} from './util.js';
import { bus } from './index';

/*
reasons for separate headers and payload on msg roots
payload separate so you can validate args on a fixed structure in js
headers for same reason, but modeled as a dictionary so serialisation to backend is easier than a combinatorial explosion of models
 */

export class MessageEnvelope {
  constructor(msg, conversationId) {
    validateArgs([{ msg }, types.object], [{ conversationId }, types.string]);

    const { schema, busChannel, ...payload } = msg;

    this.payload = payload;
    this.headers = {
      schema,
      channel: busChannel,
      conversationId,
    };
    if (msg instanceof ApiQuery) {
      this.headers.queryHash = md5Hash(msg);
    }
  }
}

export class ApiMessage {}

export class ApiCommand extends ApiMessage {
  get busChannel() {
    return bus.channels.commands;
  }
}

export class ApiQuery extends ApiCommand {
  get busChannel() {
    return bus.channels.queries;
  }
}

export class ApiEvent extends ApiMessage {}

//* Example of Actual Message with nested classes and .NET name

class TestEvent extends ApiEvent {
  constructor({ results }) {

    validateArgs(
        [{ results }, [Results]]
    );
    
    super();
    this.$type = 'Soap.TestEvent, AssemblyName'; //TODO:
    this.results = results;
  }
}
class Results {
  constructor({id}) {

    validateArgs(
        [{ id }, types.number]
    );
    
    this.$type = 'Soap.TestEvent+Results, AssemblyName';
    this.id = id;
  }
}
TestEvent.Results = Results;

export { TestEvent };
