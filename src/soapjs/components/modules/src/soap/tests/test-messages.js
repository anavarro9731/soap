import {types, validateArgs} from '../util';


/* Js version of .NET message classes with nested classes and .NET names which would be serialised and sent to this code via a JSON schema array
possible it would have been better just to put JSON in here rather than classes which have to be serialised it was hard to say which
approach would produce more reliable tests long term both have their advantages in that regards */ 

class TestCommand_c100v1 {
    constructor() {
        this.$type = 'Soap.Api.Sample.Messages.Commands.TestCommand_c100v1, Soap.Messages';
        this.c100_pointlessProp = "string";
        this.headers = [new Enumeration()];
    }
    convertToAnonymousObject() {
        return JSON.parse(JSON.stringify(this));
    }
}

class TestEvent_e200v1 {
    constructor() {
        
        this.$type = 'Soap.Api.Sample.Messages.Events.TestEvent_e200v1, Soap.Api.Sample.Messages';
        this.e200_results = [new Results()];
        this.headers = [new Enumeration()];
    }
}


class Results {
    constructor() {
        this.$type = 'Soap.Api.Sample.Messages.Events.TestEvent_e200v1+Results, Soap.Api.Sample.Messages';
        this.e200_id = 255;
    }
}
//* would normally be nested in C#
TestEvent_e200v1.Results = Results;

class Enumeration {
    constructor() {
        this.$type = "Soap.Interfaces.Messages.Enumeration, Soap.Interfaces.Messages";
        this.active = true;
        this.value = "string";
        this.key = "string";
    }
}


function convertToAnonymousObject(o) {
    return JSON.parse(JSON.stringify(o));
}

const messageSchemaArray = [
    convertToAnonymousObject(new TestCommand_c100v1()),
    convertToAnonymousObject(new TestEvent_e200v1())
];

export default messageSchemaArray;