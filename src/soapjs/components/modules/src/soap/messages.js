import {parseDotNetShortAssemblyQualifiedName, types, validateArgs} from './util';
import config from './config'

let messageTypesSingleton = {};

export function getListOfRegisteredMessages() {

    return Object.keys(messageTypesSingleton);
}

export function getRegisteredMessageType(typeName) {
    return messageTypesSingleton[typeName];
}

export const headerKeys = {
    messageId: "MessageId",
    timeOfCreationAtOrigin: "TimeOfCreationAtOrigin",
    commandConversationId: "CommandConversationId",
    commandHash: "CommandHash",
    identityToken: "IdentityToken",
    queueName: "QueueName",
    schema: "Schema",
    statefulProcessId: "StatefulProcessId",
    topic: "Topic",
    blobId: "BlobId",
    sessionId: "SessionId"
};

export const optionalStringFlag = "";
export const optionalGuidFlag = "00000000-0000-0000-0000-000000000000";
export const optionalDateFlag = "0001-01-01T00:00:00Z";
export const optionalDecimalFlag = -79228162514264337593543950335.0;
export const optionalLongFlag = -9223372036854775808;
export const optionalBoolFlag = false;
export const optionalPrimitiveFlag = "optional-primitive";

export const requiredStringFlag = "string";
export const requiredGuidFlag = "ffffffff-ffff-ffff-ffff-ffffffffffff";
export const requiredDateFlag = "9999-12-31T23:59:59.9999999Z";
export const requiredDecimalFlag = 79228162514264337593543950335.0;
export const requiredLongFlag = 9223372036854775807;
export const requiredBoolFlag = true;

export function createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousMessageObject) {

    validateArgs([{instance: anonymousMessageObject}, types.object]);

    let typedObjectWrappedInProxy = undefined;
    let typedObject = undefined;
    try {
        typedObject = makeInstanceOfCustomType(anonymousMessageObject);

        typedObjectWrappedInProxy = wrapInProxy(typedObject);

    } catch (error) {
        config.logger.log('!! ERROR converting json received from API to event !!\r\n' +  error);
        throw error;
    }

    return typedObjectWrappedInProxy;

    function makeInstanceOfCustomType(untypedMessageObject) {

        const {className} = parseDotNetShortAssemblyQualifiedName(untypedMessageObject.$type);

        //* create a function which news up an instance of this class
        const newFunction = new Function(
            'messages',
            'values',
            `return new messages["${className}"](values)`,  //* class def should already be created in messages module by defineMessageTypes()
        );

        return newFunction(messageTypesSingleton, untypedMessageObject);
    }

    /* this function adds check around property access and logs when the property you want is not there
     this is an easy way of type-checking and finding errors in code without the bloat of typescript */
    function wrapInProxy(data) {
        return new Proxy(data, {
            set: function (target, property, value) {
                // First give the target a chance to handle it
                if (Object.keys(target).indexOf(property) !== -1) {
                    return target[property];
                }
                config.logger.log(
                    `MISSING PROPERTY EXCEPTION: Attempted to write to ${typeof target}.${property.toString()} but it does not exist`,
                );
            },
            get: function (target, property) {
                // First give the target a chance to handle it
                if (
                    Object.keys(target).indexOf(property) !== -1 &&
                    property !== 'toJSON'
                ) {
                    return target[property];
                } else if (property !== 'toJSON')
                    config.logger.log(
                        `MISSING PROPERTY EXCEPTION: Attempted to read from ${typeof target}.${property.toString()} but it does not exist`,
                    );
            },
        });
    }
}

export function registerMessageTypes(arrayOfMessageSchemaAnonymousObjects) {
    arrayOfMessageSchemaAnonymousObjects.forEach(o => registerTypeDefinitionFromAnonymousObject(o));
}

export function registerTypeDefinitionFromAnonymousObject(anonymousInstance) {

    /* NOTE: This function expects default values for all properties on the object
    because it is using the instance to create a class object.
    This is different from createRegisteredTypedMessageInstanceFromAnonymousObject.
     */

    //* class def JSON is basically a JSON.NET serialised message, make sure you have not been passed null or undefined as an anonymous object
    validateArgs([{instance: anonymousInstance}, types.object]);

    const {className} = parseDotNetShortAssemblyQualifiedName(anonymousInstance.$type);

    if (messageTypesSingleton[className] === undefined) { //* create definition if not exist (some definitions of shared classes will be created)

        let typeLine = "";
        let destructureLines = "";
        let validateLines = "";
        let setterLines = "";
        let debugLines = "";

        for (let propertyName in anonymousInstance) {

            const propertyValue = anonymousInstance[propertyName];

            if (propertyName != "$type") { //* don't process this, hardcode it in the else block below

                if (propertyValue == null || propertyValue == undefined) { //* error in received json
                    throw "Cannot have null or undefined values when creating schema"
                }
                //debugLines += `console.log('According to the schema of ${className} a "${propertyName}" property should be present and of the right type on the following object provided to the constructor, validating now:', anonymousObject);`;

                destructureLines += '\t'.repeat(7) + `${propertyName},\r\n`; //* get property names

                if (isCustomType(propertyValue)) {

                    let classNameOfPropertyValue;

                    if (propertyValue instanceof Array) {
                        classNameOfPropertyValue = registerTypeDefinitionFromAnonymousObject(propertyValue[0]); //* use first element to determine properties' real type
                        if (classNameOfPropertyValue === undefined) {
                            throw "Could not register type def from anonymous object for property " + propertyName; //* console.log(propertyValue[0])
                        }
                        //* map to typed version
                        validateLines += '\t'.repeat(7) + `[{ ${propertyName} : !!${propertyName} ? ${propertyName}.map(el => new messageTypesSingleton["${classNameOfPropertyValue}"](el)) : undefined }, [messageTypesSingleton["${classNameOfPropertyValue}"]]],\r\n`;
                        setterLines += '\t'.repeat(6) + `this.${propertyName} =  ${propertyName}.map(el => new messageTypesSingleton["${classNameOfPropertyValue}"](el));\r\n`; //* set the property to the real type avoid nulls

                    } else {
                        classNameOfPropertyValue = registerTypeDefinitionFromAnonymousObject(propertyValue);
                        if (classNameOfPropertyValue === undefined) {
                            throw "Could not register type def from anonymous object for property " + propertyName; //* console.log(propertyValue[0])

                        }

                        validateLines += '\t'.repeat(7) + `[{ ${propertyName} : !!${propertyName} ? new messageTypesSingleton["${classNameOfPropertyValue}"](${propertyName}) : undefined }, messageTypesSingleton["${classNameOfPropertyValue}"]],\r\n`;  //* validate the conversion
                        setterLines += '\t'.repeat(6) + `this.${propertyName} = new messageTypesSingleton["${classNameOfPropertyValue}"](${propertyName});\r\n`; //* set the property to the real type avoid nulls
                    }
                } else { 
                    /* primitives (are always optional since they all resolve to .NET nullable types, not only does solve issues with unintended defaults values being set on deserialisaton, or
                    in the case of guids and dates not even being handleable.
                     */

                    if (propertyValue != 'optional-primitive') {
                        if (propertyValue instanceof Array) {
                            const optional = isOptional(propertyValue[0]) ? ',true' : '';
                            validateLines += '\t'.repeat(7) + `[{ ${propertyName} }, [this.types.${typeof propertyValue}] ${optional}],\r\n`; //* cannot directly use typeof or calculation in ctor calls would be dynamic
                        } else {
                            const optional = isOptional(propertyValue) ? ',true' : '';
                            validateLines += '\t'.repeat(7) + `[{ ${propertyName} }, this.types.${typeof propertyValue} ${optional}],\r\n`; //* cannot directly use typeof or calculation in ctor calls would be dynamic
                        }
                    }
                    setterLines += '\t'.repeat(6) + `this.${propertyName} = ${propertyName};\r\n`;

                }
            } else {
                typeLine = `this.$type="${propertyValue}";\r\n`
            }
        }

        const createClass = `
                messageTypesSingleton["${className}"] = class {
                    constructor(objectValues) {
                        this.validate([{ objectValues }, this.types.object]);
                        
                        const {
${destructureLines}                        } = objectValues;
${debugLines}
                        this.validate(
${validateLines}                        );

${setterLines}        
${typeLine}
                    }                 
                };
                Object.defineProperty (messageTypesSingleton["${className}"], 'name', {value: "${className}"});
                `;
        if (config.logClassDeclarations)config.logger.log(createClass);
        eval(createClass);

        messageTypesSingleton[className].prototype.validate = (...args) => {  //* be very careful arrow functions must have rest param ... to capture all arguments

            try {
                validateArgs(...args); //* and then spread them out again or you will get an extra array wrapped around the arguments and they will be passed as one argument rather then them passed as a list of args
            } catch (err) {
                throw `Error initialising class ${className}: \r\n${err}`;
            }
        };  //* wasn't sure how else to get these in scope may be a better way
        messageTypesSingleton[className].prototype.types = types;

    }
    return className;

    function isOptional(contestant) {

        const optionalValues = [
            optionalStringFlag,
            optionalGuidFlag,
            optionalDateFlag,
            optionalDecimalFlag,
            optionalLongFlag,
            optionalBoolFlag,
            optionalPrimitiveFlag
        ];

        return contains(optionalValues, contestant)

        function contains(a, obj) {
            for (let i = 0; i < a.length; i++) {
                if (a[i] === obj) {
                    return true;
                }
            }
            return false;
        }
    }
    

    
    function isCustomType(value) {

        if (value instanceof Array) {
            return isObjectAndNotPrimitive(value[0]);
        } else {
            return isObjectAndNotPrimitive(value);
        }

        function isObjectAndNotPrimitive(value) {
            return typeof value === 'object' && value !== undefined && value !== null;
        }
    }

}
