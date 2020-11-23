import {
    boolMax,
    boolMin,
    byteMax,
    byteMin,
    dateMax,
    dateMin,
    decimalMax,
    decimalMin,
    guidMax,
    guidMin,
    intMax,
    intMin,
    longMax,
    longMin,
    shortMax,
    shortMin,
    stringMin,
    stringPresent
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
                this.c101_Boolean = boolMax
                this.c101_BooleanOptional = boolMin
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringMin;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMin;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMin;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMin
                this.c101_Long = longMax;
                this.c101_LongOptional = longMin;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMin;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMin;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMin;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [new Enumeration()];
                break;
            case "min": //* should all work
                this.c101_Boolean = boolMin;
                this.c101_BooleanOptional = boolMin;
                this.c101_String == stringMin;
                this.c101_StringOptional = stringMin;
                this.c101_Integer = intMin;
                this.c101_IntegerOptional = intMin;
                this.c101_Short = shortMin;
                this.c101_ShortOptional = shortMin;
                this.c101_Byte = byteMin;
                this.c101_ByteOptional = byteMin
                this.c101_Long = longMin;
                this.c101_LongOptional = longMin;
                this.c101_Decimal = decimalMin;
                this.c101_DecimalOptional = decimalMin;
                this.c101_Guid = guidMin;
                this.c101_GuidOptional = guidMin;
                this.c101_DateTime = dateMin;
                this.c101_DateTimeOptional = dateMin;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = []; //* we do allow empty lists
                break;
            case "max": //* should all work
                this.c101_Boolean = boolMax;
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMax;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMax;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMax
                this.c101_Long = longMax;
                this.c101_LongOptional = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [new Enumeration(), new Enumeration()];
                break;
            case "optionals": //* should pass
                this.c101_Boolean = boolMax;
                this.c101_String = stringPresent;
                this.c101_Integer = intMax;
                this.c101_Short = shortMax;
                this.c101_Byte = byteMax
                this.c101_Long = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_BooleanOptional = undefined;
                this.c101_StringOptional = undefined;
                this.c101_IntegerOptional = undefined;
                this.c101_ShortOptional = undefined;
                this.c101_ByteOptional = undefined
                this.c101_LongOptional = undefined;
                this.c101_DecimalOptional = undefined;
                this.c101_GuidOptional = undefined;
                this.c101_DateTimeOptional = undefined;
                break;
            case "required": //* should pass
                this.c101_Boolean = undefined;
                this.c101_String = undefined;
                this.c101_Integer = undefined;
                this.c101_Short = undefined;
                this.c101_Byte = undefined
                this.c101_Long = undefined;
                this.c101_Decimal = undefined;
                this.c101_Guid = undefined;
                this.c101_DateTime = undefined;
                this.c101_BooleanOptional = undefined;
                this.c101_StringOptional = undefined;
                this.c101_IntegerOptional = undefined;
                this.c101_ShortOptional = undefined;
                this.c101_ByteOptional = undefined
                this.c101_LongOptional = undefined;
                this.c101_DecimalOptional = undefined;
                this.c101_GuidOptional = undefined;
                this.c101_DateTimeOptional = undefined;
                break;
            case "emptylist": //* pass list
                this.c101_Boolean = boolMax;
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMax;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMax;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMax
                this.c101_Long = longMax;
                this.c101_LongOptional = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [];
                break;
            case "nullinlist": //* fail list
                this.c101_Boolean = boolMax;
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMax;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMax;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMax
                this.c101_Long = longMax;
                this.c101_LongOptional = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [null];
                break;
            case "undefinedinlist": //* fail list
                this.c101_Boolean = boolMax;
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMax;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMax;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMax
                this.c101_Long = longMax;
                this.c101_LongOptional = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMax;
                this.c101_Object = new Enumeration();
                this.c101_ListOfObjects = [undefined];
                break;
            case "childcontainsundefined":
                this.c101_Boolean = boolMax;
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMax;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMax;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMax
                this.c101_Long = longMax;
                this.c101_LongOptional = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMax;
                const brokenChild1 = new Enumeration();
                brokenChild1.value = undefined;
                this.c101_Object = brokenChild1;
                this.c101_ListOfObjects = [new Enumeration()];
                break;
            case "childcontainsnull":
                this.c101_Boolean = boolMax;
                this.c101_BooleanOptional = boolMax;
                this.c101_String == stringPresent;
                this.c101_StringOptional = stringPresent;
                this.c101_Integer = intMax;
                this.c101_IntegerOptional = intMax;
                this.c101_Short = shortMax;
                this.c101_ShortOptional = shortMax;
                this.c101_Byte = byteMax;
                this.c101_ByteOptional = byteMax
                this.c101_Long = longMax;
                this.c101_LongOptional = longMax;
                this.c101_Decimal = decimalMax;
                this.c101_DecimalOptional = decimalMax;
                this.c101_Guid = guidMax;
                this.c101_GuidOptional = guidMax;
                this.c101_DateTime = dateMax;
                this.c101_DateTimeOptional = dateMax;
                const brokenChild2 = new Enumeration();
                brokenChild2.value = null;
                brokenChild2.key = null;    
                this.c101_Object = brokenChild2;
                this.c101_ListOfObjects = [new Enumeration()];
                break;
        }

        function setPassingDefaults($this) {
            $this.c101_Boolean = boolMax;
            $this.c101_BooleanOptional = boolMax;
            $this.c101_String == stringPresent;
            $this.c101_StringOptional = stringPresent;
            $this.c101_Integer = intMax;
            $this.c101_IntegerOptional = intMax;
            $this.c101_Short = shortMax;
            $this.c101_ShortOptional = shortMax;
            $this.c101_Byte = byteMax;
            $this.c101_ByteOptional = byteMax
            $this.c101_Long = longMax;
            $this.c101_LongOptional = longMax;
            $this.c101_Decimal = decimalMax;
            $this.c101_DecimalOptional = decimalMax;
            $this.c101_Guid = guidMax;
            $this.c101_GuidOptional = guidMax;
            $this.c101_DateTime = dateMax;
            $this.c101_DateTimeOptional = dateMax;
            $this.c101_Object = new Enumeration();
            $this.c101_ListOfObjects = [new Enumeration(), new Enumeration()];
        };
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