const stringNullable = "";
const guidMin = "00000000-0000-0000-0000-000000000000";
const dateMin = "0001-01-01T00:00:00Z";
const decimalMin = -79228162514264337593543950335.0;
const longMin = -9223372036854775808;
const boolMin = false;

const stringPresent = "string";
const guidMax = "ffffffff-ffff-ffff-ffff-ffffffffffff";
const dateMax = "9999-12-31T23:59:59.9999999Z";
const decimalMax = 79228162514264337593543950335.0;
const longMax = 9223372036854775807;
const boolMax = true;

/* Js version of .NET message classes with nested classes and .NET names which would be serialised and sent to this code via a JSON schema array
possible it would have been better just to put JSON in here rather than classes which have to be serialised it was hard to say which
approach would produce more reliable tests long term both have their advantages in that regards 

this.headers and this.$type is required on all messages */

class TestCommand_c100v1 {
    constructor() {
        this.$type = 'Soap.Api.Sample.Messages.Commands.TestCommand_c100v1, Soap.Messages';
        this.c100_pointlessProp = "string";
        this.headers = [new Enumeration()];
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


export class TestCommand_c101v1 {
    constructor(scenario) {

        this.$type = 'Soap.Api.Sample.Messages.Commands.TestCommand_c101v1, Soap.Messages';
        this.headers = [new Enumeration()];
        
        switch (scenario) {
            default: //* schema
                this.c101_BooleanOptional = boolMin
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringNullable;
                this.c101_LongOptional = longMin;
                this.c101_DecimalOptional = decimalMin;
                this.c101_GuidOptional = guidMin;
                this.c101_DateTimeOptional = dateMin;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [new Enumeration()];
                break;
            case "min": //* should all work
                this.c101_BooleanOptional = boolMin;
                this.c101_String == stringNullable;
                this.c101_StringOptional = stringNullable;
                this.c101_LongOptional = longMin;
                this.c101_DecimalOptional = decimalMin;
                this.c101_GuidOptional = guidMin;
                this.c101_DateTimeOptional = dateMin;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = []; //* we do allow empty lists
                break;
            case "max": //* should all work
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_LongOptional = longMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [new Enumeration(), new Enumeration()];
                break;
            case "optionals": //* should pass
                this.c101_String = stringPresent;
                this.c101_BooleanOptional = undefined;
                this.c101_StringOptional = undefined;
                this.c101_LongOptional = undefined;
                this.c101_DecimalOptional = undefined;
                this.c101_GuidOptional = undefined;
                this.c101_DateTimeOptional = undefined;
                break;
            case "required": //* should pass
                this.c101_String = undefined;
                this.c101_BooleanOptional = undefined;
                this.c101_StringOptional = undefined;
                this.c101_LongOptional = undefined;
                this.c101_DecimalOptional = undefined;
                this.c101_GuidOptional = undefined;
                this.c101_DateTimeOptional = undefined;
                break;
            case "emptylist": //* pass list
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_LongOptional = longMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [];
                break;
            case "nullinlist": //* fail list
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_LongOptional = longMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [null];
                break;
            case "undefinedinlist": //* fail list
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_LongOptional = longMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [undefined];
                break;
            case "childcontainsundefined":
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_LongOptional = longMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTimeOptional = dateMax;
                const brokenChild1 = new Enumeration();
                brokenChild1.value = undefined;
                this.c101_Object = brokenChild1;
                this.c101_ListOfObjects = [new Enumeration()];
                break;
            case "childcontainsnull":
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_LongOptional = longMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTimeOptional = dateMax;
                const brokenChild2 = new Enumeration();
                brokenChild2.value = null;
                brokenChild2.key = null;    
                this.c101_Object = brokenChild2;
                this.c101_ListOfObjects = [new Enumeration()];
                break;
        }
    }
}


function convertToAnonymousObject(o) {
    return JSON.parse(JSON.stringify(o));
}

const messageSchemaArray = [
    convertToAnonymousObject(new TestCommand_c100v1()),
    convertToAnonymousObject(new TestEvent_e200v1()),
    convertToAnonymousObject(new TestCommand_c101v1())
];

export default messageSchemaArray;
