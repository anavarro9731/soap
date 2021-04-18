import FileView from "./FileView";
import React from "react";
import {ArrayTableNested, ObjectTableNested} from "./Tables";

const blobMetaMarkerGuid = "20fb62ff-9dd3-436e-a356-eceb335c2572";

export function ConvertObjectToComponentArray(object, propertyRenderer) {

    const arrayOfComponentsFromObjectProperties = [];

    for (const nameOfProperty in object) {
        if (object.hasOwnProperty(nameOfProperty)) {
            if (nameOfProperty !== "validate" && nameOfProperty !== "types" && nameOfProperty !== "$type") {

                const propertyValue = object[nameOfProperty];

                if (propertyValue instanceof Array) {
                    arrayOfComponentsFromObjectProperties.push(<ArrayTableNested arrayOfObjects={propertyValue}
                                                                                 propertyRenderer={propertyRenderer}/>);
                } else if (isChildObject(propertyValue)) {
                    arrayOfComponentsFromObjectProperties.push(<ObjectTableNested obj={object}
                                                                                  propertyRenderer={propertyRenderer}/>)
                } else {
                    arrayOfComponentsFromObjectProperties.push(ConvertPropertyToComponent(nameOfProperty, propertyValue, propertyRenderer));
                }
            }
        }
    }
    return arrayOfComponentsFromObjectProperties;

    function isChildObject(propertyValue) {
        return typeof propertyValue === typeof {} &&
        propertyValue !== null &&
        propertyValue.blobMetaMarker !== blobMetaMarkerGuid;
    }
    
    function ConvertPropertyToComponent(nameOfProperty, propertyValue, propertyRenderer) {
        
        let value;

        if (typeof propertyValue === typeof '') {
            //string
            value = propertyValue
        } else if (typeof propertyValue === typeof true) {
            //true
            value = propertyValue.toString();
        } else if (typeof propertyValue === typeof 1) {
            //number
            value = propertyValue.toString();
        } else if (typeof propertyValue === typeof undefined) {
            //undefined
            value = '- - -';
        } else if (typeof propertyValue === typeof {}) {
            if (propertyValue === null) {
                //null
                value = '- - -';
            } else if (propertyValue.blobMetaMarker === blobMetaMarkerGuid) {
                //blob
                value = <FileView value={propertyValue}/>;
            } else {
                value = "## error ##";
            }
        }
        if (propertyRenderer && propertyRenderer[nameOfProperty]) {
            return  propertyRenderer[nameOfProperty](value);
        } else {
            return value;
        }
    }
}