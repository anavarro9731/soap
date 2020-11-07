import {types, validateArgs} from '../util';
import {ApiMessage} from "../messages";

//* Example of Actual Message with nested classes and .NET name

export class TestCommand_c100v1 extends ApiMessage {
    constructor({c100_pointlessProp}) {

        validateArgs(
            [{c100_pointlessProp}, types.string]
        );

        super();
        this.$type = 'Soap.Messages.Commands.TestCommand_c100v1, Soap.Messages';
        this.c100_pointlessProp = c100_pointlessProp;
    }
}

class TestEvent_e200v1 extends ApiMessage {
    constructor({e200_results}) {

        validateArgs(
            [{e200_results}, [Results]]
        );

        super();
        this.$type = 'Soap.Messages.Events.TestEvent_e200v1, Soap.Messages';
        this.e200_results = e200_results;
    }
}

class Results {
    constructor({e200_id}) {

        validateArgs(
            [{e200_id}, types.number]
        );

        this.$type = 'Soap.Messages.Events.TestEvent_e200v1+Results, Soap.Messages';
        this.e200_id = e200_id;
    }
}

TestEvent_e200v1.Results = Results;

export {TestEvent_e200v1};