import { types, validateArgs, md5Hash } from './util.js';
import { bus } from './index';

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

export class TestEvent_Results {
  constructor(id) {
    this.$type = 'TestEvent_Results';
    this.id = id;
  }
}

export class TestEvent extends ApiEvent {
  constructor(resultIds) {
    super();
    this.$type = 'TestEvent';
    if (!!resultIds)
      this.resultIds = resultIds.map(x => new TestEvent_Results(x));
  }
}
