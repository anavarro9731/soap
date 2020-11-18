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
    messageId : "MessageId",
    timeOfCreationAtOrigin: "TimeOfCreationAtOrigin",
    commandConversationId: "CommandConversationId",
    commandHash: "CommandHash",
    identityToken: "IdentityToken",
    queueName: "QueueName",
    schema: "Schema",
    statefulProcessId: "StatefulProcessId",
    topic: "Topic",
    blobId: "BlobId"
};

export function createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousMessageObject) {

    validateArgs([{instance: anonymousMessageObject}, types.object]);

    let typedObjectWrappedInProxy = undefined;
    let typedObject = undefined;
    try {
        typedObject = makeInstanceOfCustomType(anonymousMessageObject);
        
        typedObjectWrappedInProxy = wrapInProxy(typedObject);

    } catch (error) {
        config.logger.log('!! ERROR converting json received from API to event !! ', error);
        throw error;
    }

    return typedObjectWrappedInProxy;

    function makeInstanceOfCustomType(untypedMessageObject) {

        const { classType } = parseDotNetShortAssemblyQualifiedName(untypedMessageObject.$type);
        
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

    //* class def JSON is basically a JSON.NET serialised message
    validateArgs([{instance: anonymousInstance}, types.object]);
    
    const {className} = parseDotNetShortAssemblyQualifiedName(anonymousInstance.$type);

    if (messageTypesSingleton[className] === undefined) { //* create definition if not exist (some definitions of shared classes will be created)

        let classBody = "";

        for (let propertyName in anonymousInstance) {

            const propertyValue = anonymousInstance[propertyName];
            
            if (propertyName != "$type") { //* don't process this, hardcode it in the else block below

                if (propertyValue == null || propertyValue == undefined) { //* error in received json
                    throw "Cannot have null or undefined values when creating schema"
                }
                //classBody += `log('According to the schema of ${className} a "${propertyName}" property should be present and of the right type on the following object provided to the constructor, validating now:', anonymousObject);`;
                
                classBody += `const {${propertyName}} = anonymousObject;\r\n`; //* destructure anonymousObject defined in class ctor below
                
                if (isCustomType(propertyValue)) {

                    let classNameOfPropertyValue;

                    if (propertyValue instanceof Array) {
                            classNameOfPropertyValue = registerTypeDefinitionFromAnonymousObject(propertyValue[0]); //* use first element to determine properties' real type
                            if (classNameOfPropertyValue === undefined) {
                                //log("Could not register type def from anonymous object for property " + propertyName, propertyValue[0]);
                                throw "Could not register type def from anonymous object for property " + propertyName;
                            }
                            classBody += `this.validate([{ ${propertyName} }, [this.types.object]]);\r\n`;  //* validate the conversion
                            classBody += `const typed${propertyName} = ${propertyName}.map(el => new messageTypesSingleton["${classNameOfPropertyValue}"](el));\r\n`; //*  convert it to an array of typed items
                            classBody += `this.validate([{ typed${propertyName} }, [messageTypesSingleton["${classNameOfPropertyValue}"]]]);\r\n`;  //* validate the conversion    
                        
                    } else {
                        
                        classNameOfPropertyValue = registerTypeDefinitionFromAnonymousObject(propertyValue);
                        if (classNameOfPropertyValue === undefined) {
                            //log("Could not register type def from anonymous object for property " + propertyName, propertyValue);
                            throw "Could not register type def from anonymous object for property " + propertyName;
                        }
                        classBody += `const typed${propertyName} = new messageTypesSingleton["${classNameOfPropertyValue}"](${propertyName});\r\n`; //*  convert it to a typed item
                        classBody += `this.validate([{ typed${propertyName} }, messageTypesSingleton["${classNameOfPropertyValue}"]]);\r\n`;  //* validate the conversion
                    }

                    classBody += `this.${propertyName} = typed${propertyName} === null ? undefined : typed${propertyName};\r\n`; //* set the property to the real type avoid nulls

                } else { //* primitives

                    if (propertyValue instanceof Array) {
                         
                            const optional = isOptional(propertyValue[0]) ? ',true' : '';
                            classBody += `this.validate([{ ${propertyName} }, [this.types.${typeof propertyValue}]] ${optional});\r\n`; //* cannot directly use typeof or calculation in ctor calls would be dynamic
                    } else {
                        const optional = isOptional(propertyValue) ? ',true' : '';
                        classBody += `this.validate([{ ${propertyName} }, this.types.${typeof propertyValue}] ${optional});\r\n`; //* cannot directly use typeof or calculation in ctor calls would be dynamic
                    }
                    classBody += `this.${propertyName} = ${propertyName};\r\n`;
                    
                }
            } else {
                classBody += `this.$type="${propertyValue}";\r\n`
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

        messageTypesSingleton[className].prototype.validate = args => {
            try {
                validateArgs(args);
            } catch(err) {
                throw `Error initialising class ${className}: ${err}`;  
            }
        };  //* wasn't sure how else to get these in scope may be a better way
        messageTypesSingleton[className].prototype.types = types;
        
    }
    return className;

    function isOptional(contestant) {

        const optionalValues = [
            "", //*string 
            "00000000-0000-0000-0000-000000000000", //*guid
            "0001-01-01T00:00:00Z", //*datetime
            -79228162514264337593543950335.0, //*decimal
            -9223372036854775808, //*long
            -2147483648, //*int
            -32769, //*short
            0, //*byte
            -128, //*sbyte
            false, //*bool
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
            return isObjectAndNotPrimitive(value[0]) ;
        } else {
            return isObjectAndNotPrimitive(value) ;
        }

        function isObjectAndNotPrimitive(value) {
            return typeof value === 'object' && value !== undefined;
        }
    }

}