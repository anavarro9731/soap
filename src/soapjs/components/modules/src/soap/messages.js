import {convertDotNetAssemblyQualifiedNameToJsClassName, types, validateArgs} from './util';
import config from './config'

let messageTypesSingleton = {};

export function getListOfRegisteredMessages() {
    return Object.keys(messageTypesSingleton);
}

export function getRegisteredMessageType(typeName) {
    return messageTypesSingleton[typeName];
}

export function createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousMessageObject) {

    validateArgs([{instance: anonymousMessageObject}, types.object]);

    let typedEventWrappedInProxy = undefined;
    let typedEvent = undefined;
    try {
        typedEvent =  makeInstanceOfCustomType(anonymousMessageObject);
        typedEventWrappedInProxy = wrapInProxy(typedEvent);

    } catch (error) {
        config.log('!! ERROR converting json received from API to event !! ', error);
        throw error;
    }
    
    return typedEventWrappedInProxy;

    function makeInstanceOfCustomType(untypedMessageObject) {

        const classType = convertDotNetAssemblyQualifiedNameToJsClassName(untypedMessageObject.$type);

        //* create a function which news up an instance of this class
        const newFunction = new Function(
            'messages',
            'values',
            `return new messages["${classType}"](values)`,  //* class def should already be created in messages module by defineMessageTypes()
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
                config.log(
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
                    config.log(
                        `MISSING PROPERTY EXCEPTION: Attempted to read from ${typeof target}.${property.toString()} but it does not exist`,
                    );
            },
        });
    }
}

export function registerMessageTypes(arrayOfMessageSchemaAnonymousObjects) {
    arrayOfMessageSchemaAnonymousObjects.forEach(o => registerTypeDefinitionFromAnonymousObject(o));
}

export function registerTypeDefinitionFromAnonymousObject(anonymousMessageObject) {

    /* NOTE: This function expects default values for all properties on the object
    because it is using the instance to create a class object.
    This is different from createRegisteredTypedMessageInstanceFromAnonymousObject.
     */
    
    //* class def JSON is basically a JSON.NET serialised message
    validateArgs([{instance: anonymousMessageObject}, types.object]);

    const className = convertDotNetAssemblyQualifiedNameToJsClassName(anonymousMessageObject.$type);

    if (!messageTypesSingleton[className]) { //* create definition if not exist (some definitions of shared classes will be created)

        let classBody = "";

        for (let propertyName in anonymousMessageObject) {

            let propertyValue = anonymousMessageObject[propertyName];

            if (propertyValue == null || propertyValue == undefined) {
                throw "Cannot have null or undefined values when creating schema"
            }

            classBody += `const {${propertyName}} = anonymousObject;\r\n`; //* destructure anonymousObject

            if (isCustomType(propertyValue)) {

                let classNameOfPropertyValue;

                if (propertyValue instanceof Array) {
                    classNameOfPropertyValue = registerTypeDefinitionFromAnonymousObject(propertyValue[0]); //* use first element to determine properties' real type
                    classBody += `const typed${propertyName} = ${propertyName}.map(el => new messageTypesSingleton["${classNameOfPropertyValue}"](el));\r\n`; //*  convert it to an array of typed items
                    classBody += `this.validate([{ typed${propertyName} }, [messageTypesSingleton["${classNameOfPropertyValue}"]]]);\r\n`;  //* validate the conversion
                } else {
                    classNameOfPropertyValue = registerTypeDefinitionFromAnonymousObject(propertyValue);
                    classBody += `const typed${propertyName} = new messageTypesSingleton[${classNameOfPropertyValue}](${propertyName});\r\n`; //*  convert it to a typed item
                    classBody += `this.validate([{ typed${propertyName} }, messageTypesSingleton["${classNameOfPropertyValue}"]]);\r\n`;  //* validate the conversion
                }

                classBody += `this.${propertyName} = typed${propertyName} === null ? undefined : typed${propertyName};\r\n`; //* set the property to the real type avoid nulls

            } else {
                classBody += `this.validate([{ ${propertyName} }, this.types.${typeof propertyValue}]);\r\n`; //* cannot directly use typeof or calculation in ctor calls would be dynamic
                classBody += `this.${propertyName} = ${propertyName};\r\n`;
            }
        }

        const createClass = `
                messageTypesSingleton["${className}"] = class {
                    constructor(anonymousObject) {
                        ${classBody}    
                    }                 
                }
                Object.defineProperty (messageTypesSingleton["${className}"], 'name', {value: "${className}"});
                `;

        eval(createClass);

        messageTypesSingleton[className].prototype.validate = validateArgs;  //* wasn't sure how else to get these in scope may be a better way
        messageTypesSingleton[className].prototype.types = types;

        return className;
    }


    function isCustomType(value) {

        if (value instanceof Array) {
            return isObjectAndNotPrimitive(value[0]) && hasItsTypeDefined(value[0]);
        } else {
            return isObjectAndNotPrimitive(value) && hasItsTypeDefined(value);
        }

        function hasItsTypeDefined(value) {
            return value.hasOwnProperty('$type')
        }

        function isObjectAndNotPrimitive(value) {
            return typeof value === 'object' && value !== null;
        }
    }

}