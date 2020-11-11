import {types, validateArgs} from '../util';


//* Example of Actual Message with nested classes and .NET name

export class TestCommand_c100v1 {
    constructor({c100_pointlessProp}) {

        validateArgs(
            [{c100_pointlessProp}, types.string]
        );
        
        this.$type = 'Soap.Messages.Commands.TestCommand_c100v1, Soap.Messages';
        this.c100_pointlessProp = c100_pointlessProp;
        this.headers = {};
    }

    static CreateTemplate() {
        return new this({c100_pointlessProp: '12345'});
    }
    
    convertToAnonymousObject() {
        return JSON.parse(JSON.stringify(this));
    }
}

class TestEvent_e200v1 {
    constructor({e200_results}) {

        validateArgs(
            [{e200_results}, [Results], true]
        );
        
        this.$type = 'Soap.Messages.Events.TestEvent_e200v1, Soap.Messages';
        this.e200_results = e200_results;
        this.headers = {};
    }

    static CreateTemplate() {
        return new this({e200_results: [new Results({e200_id: 1})]});
    }
    
    convertToAnonymousObject() {
        return JSON.parse(JSON.stringify(this));
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