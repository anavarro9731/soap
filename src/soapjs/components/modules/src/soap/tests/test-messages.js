import {
    requiredBoolFlag,
    optionalBoolFlag,
    requiredDateFlag,
    optionalDateFlag,
    requiredDecimalFlag,
    optionalDecimalFlag,
    requiredGuidFlag,
    optionalGuidFlag,
    requiredLongFlag,
    optionalLongFlag,
    optionalStringFlag,
    requiredStringFlag
} from '../messages'

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
                this.c101_Boolean = requiredBoolFlag
                this.c101_BooleanOptional = optionalBoolFlag
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = optionalStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = optionalLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = optionalDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = optionalGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = optionalDateFlag;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [new Enumeration()];
                break;
            case "min": //* should all work
                this.c101_Boolean = optionalBoolFlag;
                this.c101_BooleanOptional = optionalBoolFlag;
                this.c101_String = optionalStringFlag;
                this.c101_StringOptional = optionalStringFlag;
                this.c101_Long = optionalLongFlag;
                this.c101_LongOptional = optionalLongFlag;
                this.c101_Decimal = optionalDecimalFlag;
                this.c101_DecimalOptional = optionalDecimalFlag;
                this.c101_Guid = optionalGuidFlag;
                this.c101_GuidOptional = optionalGuidFlag;
                this.c101_DateTime = optionalDateFlag;
                this.c101_DateTimeOptional = optionalDateFlag;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = []; //* we do allow empty lists
                break;
            case "max": //* should all work
                this.c101_Boolean = requiredBoolFlag;
                this.c101_BooleanOptional = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = requiredDateFlag;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [new Enumeration(), new Enumeration()];
                break;
            case "optionalAreOptional": //* should pass
                this.c101_Boolean = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_BooleanOptional = undefined;
                this.c101_StringOptional = undefined;
                this.c101_LongOptional = undefined;
                this.c101_DecimalOptional = undefined;
                this.c101_GuidOptional = undefined;
                this.c101_DateTimeOptional = undefined;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [];
                break;
            case "requiredIsRequiredUndefined": //* should pass
                this.c101_Boolean = undefined;
                this.c101_String = undefined;
                this.c101_Long = undefined;
                this.c101_Decimal = undefined;
                this.c101_Guid = undefined;
                this.c101_DateTime = undefined;
                this.c101_BooleanOptional = undefined;
                this.c101_StringOptional = undefined;
                this.c101_LongOptional = undefined;
                this.c101_DecimalOptional = undefined;
                this.c101_GuidOptional = undefined;
                this.c101_DateTimeOptional = undefined;
                this.c101_Object = undefined;
                this.c101_ListOfObjects = undefined;
                break;
            case "requiredIsRequiredNull": //* should pass
                this.c101_Boolean = null;
                this.c101_String = null;
                this.c101_Long = null;
                this.c101_Decimal = null;
                this.c101_Guid = null;
                this.c101_DateTime = null;
                this.c101_BooleanOptional = null;
                this.c101_StringOptional = null;
                this.c101_LongOptional = null;
                this.c101_DecimalOptional = null;
                this.c101_GuidOptional = null;
                this.c101_DateTimeOptional = null;
                this.c101_Object = null;
                this.c101_ListOfObjects = null;
                break;
            case "emptylist": //* pass list
                this.c101_Boolean = requiredBoolFlag;
                this.c101_BooleanOptional = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = requiredDateFlag;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [];
                break;
            case "nullinlist": //* fail list
                this.c101_Boolean = requiredBoolFlag;
                this.c101_BooleanOptional = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = requiredDateFlag;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [null];
                break;
            case "undefinedinlist": //* fail list
                this.c101_Boolean = requiredBoolFlag;
                this.c101_BooleanOptional = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = requiredDateFlag;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [undefined];
                break;
            case "childcontainsundefined":
                this.c101_Boolean = requiredBoolFlag;
                this.c101_BooleanOptional = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = requiredDateFlag;
                const brokenChild1 = new Enumeration();
                brokenChild1.value = undefined;
                this.c101_Object = brokenChild1;
                this.c101_ListOfObjects = [new Enumeration()];
                break;
            case "childcontainsnull":
                this.c101_Boolean = requiredBoolFlag;
                this.c101_BooleanOptional = requiredBoolFlag;
                this.c101_String = requiredStringFlag;
                this.c101_StringOptional = requiredStringFlag;
                this.c101_Long = requiredLongFlag;
                this.c101_LongOptional = requiredLongFlag;
                this.c101_Decimal = requiredDecimalFlag;
                this.c101_DecimalOptional = requiredDecimalFlag;
                this.c101_Guid = requiredGuidFlag;
                this.c101_GuidOptional = requiredGuidFlag;
                this.c101_DateTime = requiredDateFlag;
                this.c101_DateTimeOptional = requiredDateFlag;
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
