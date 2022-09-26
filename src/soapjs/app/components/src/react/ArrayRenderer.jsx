import React, {useEffect, useState} from 'react';
import {CreateTertiaryActionsMenu, CreateViewButton} from "./ActionMenu";
import {FileView} from "./FileView";
import {Button, SIZE, KIND, SHAPE} from "baseui/button";
import DOMPurify from "dompurify";
import {
    StyledTable,
    StyledTableBodyCell,
    StyledTableBodyRow,
    StyledTableHeadCell,
    StyledTableHeadRow
} from "baseui/table-semantic";
import {uuidv4} from "../soap/util";

const DataTypes = {
    PrimitiveArray : "primitive-array",
    ObjectArray : "object-array",
    Object : "object"
};

export function ArrayRenderer(props) {

    const [expandedRows, setExpandedRows] = useState([]);
    const [initialExpansionCalculated, setInitialExpansionCalculated] = useState(false);
    
    const {
        propertyKey,
        arrayOfObjects,
        propertyRenderer,
        entityMenus,
        hiddenFields,
        expandedFields,
        expandedFieldsFirstObjectOnly,
        headerColumns,
        dataType = DataTypes.ObjectArray
    } = props;
    
    const hasMenu = entityMenus && entityMenus[propertyKey];
    const isRoot = propertyKey === "root-Items";
    
    {
        if (arrayOfObjects.length > 0) {

            //* BUILD TITLE ROW

            const firstObjectKeys = Object.keys(arrayOfObjects[0]);
            //* reduce header columns set to just those for this object
            const titleColumns = [...headerColumns.filter(x => firstObjectKeys.includes(x))];
            //* if there are none use the object type as a placeholder
            if (titleColumns.length === 0) titleColumns.push("$type");
            //* create an array to hold the components, and a column for the left side control
            let titleComponentArray = [" "];
            for (const titleColumn of titleColumns) { //* respecting the order specified in the incoming prop array
                //* add a column
                titleComponentArray.push(ConvertObjectKeyToLabel(titleColumn));
            }
            if (hasMenu) titleComponentArray.push(" "); //* add column for controls at the end


            //* BUILD THE DATA ROWS
            const rows = {
                items: [],
                dataType
            };

            //*  if this key is listed in the expanded keys prop, always expand the row
            //console.warn(1, propertyKey, initialExpansionCalculated, expandedRows, expandedFields, expandedFieldsFirstObjectOnly);
            if (!initialExpansionCalculated) {
                if (expandedFields.includes(propertyKey)) {
                    const arrayOfAllIndexes = [...Array(arrayOfObjects.length).keys()];
                    
                    setExpandedRows(arrayOfAllIndexes);
                } else if (expandedFieldsFirstObjectOnly.includes(propertyKey)) {
                    setExpandedRows([0]);
                }
                setInitialExpansionCalculated(true);
            }
        

            //* add a row for each object in array
            for (const [index, obj] of arrayOfObjects.entries()) {

                //* create empty row object
                const rowObject = {
                    rowIndex: index,
                    rowComponentArray: [],
                    panelObject: null,
                };



                //* when you render the row, if the contents are a base type treat differently
                if (rows.dataType === DataTypes.PrimitiveArray) {
                    //* base type arrays are not object so they have no properties with keys
                    // just add the item in a single column
                    rowObject.rowComponentArray.push(obj);
                } else {
                    //* add a column to row for each title column
                    for (const titleColumn of titleColumns) {
                        //* add data to the column, you could add data which is itself an array table this way 
                        // but it's not advisable from a UI perspective, it will distort the layout too much
                        rowObject.rowComponentArray.push(HandleProperty(titleColumn, obj[titleColumn]));
                    }

                    //* BUILD OBJECT PANEL
                    const panelObj = removeHiddenFields(obj, hiddenFields);
                    if (!!panelObj) rowObject.panelObject = panelObj;
                }

                rows.items.push(rowObject);
            }

            //* PRINT TO SCREEN
            

            // if (propertyKey === "e105_Countries-ArrayItems") { //useful callsite for debugging
            //     console.log("a",titleComponentArray, "b",titleColumns, "c",rows, "d",arrayOfObjects);
            // }
            
            return PrintRows(propertyKey, titleComponentArray, rows);

        } else return null;
        
    }

    function removeHiddenFields(obj, hiddenFields) {
        const hiddenFieldsEx = [...hiddenFields, "$type", "headers", "validate", "types",]; //* validate, types, headers would be present if the root event was passed in
        const objEntries = Object.entries(obj).filter(([key, _]) => !hiddenFieldsEx.includes(key)); //* select the fields not excluded from each object
        return objEntries.length > 0 ? Object.fromEntries(objEntries) : null;
    }

    function PrintRows(propertyKey, titleComponentArray, rows) {
        
        const colSpan = titleComponentArray.length;

        return (<StyledTable>
                <thead>
                {rows.dataType === DataTypes.ObjectArray ? PrintTitleRow(titleComponentArray) : null}
                </thead>
                <tbody>
                {rows.items.map((r, i) => {
                    if (rows.dataType === DataTypes.Object) {
                        return PrintObjectPanel(r.panelObject, colSpan);
                    } else {
                        const isLastRow = i === rows.items.length-1;
                        return expandedRows.includes(i)
                            ? [PrintHeaderRow(r, isLastRow), PrintObjectPanel(r.panelObject, colSpan)]
                            : PrintHeaderRow(r, isLastRow)
                    }
                })}
                </tbody>
            </StyledTable>
        );

    }
    
    function PrintHeaderRow(rowObject, isLastRow) {
        
        const {rowComponentArray, panelObject, rowIndex } = rowObject;
        
        //* add controls to the end
        if (hasMenu) rowComponentArray.push(<div style={{display: "flex"}}>
            {CreateViewButton(propertyKey, entityMenus, panelObject)}
            {CreateTertiaryActionsMenu(propertyKey, entityMenus, panelObject)}
        </div>);

        return (

            <StyledTableBodyRow key={uuidv4()}  >
                {dataType === DataTypes.PrimitiveArray ? null :
                    <StyledTableBodyCell style={isLastRow && !isRoot ? {borderBottomWidth:"0px"} : {}}>
                        {expandedRows.includes(rowIndex) ?
                            <Button size={SIZE.mini} kind={KIND.secondary} shape={SHAPE.circle} onClick={() => {
                                setExpandedRows(oldArray => oldArray.filter(x => x !== rowIndex));
                            }}>-</Button> :
                            <Button size={SIZE.mini} kind={KIND.secondary} shape={SHAPE.circle} onClick={() => {
                                setExpandedRows(oldArray => [rowIndex, ...oldArray]);
                            }}>+</Button>}
                    </StyledTableBodyCell>}
                {rowComponentArray.map(component => <StyledTableBodyCell key={uuidv4()} style={isLastRow && !isRoot ? {borderBottomWidth:"0px"} : {}}>{component}</StyledTableBodyCell>)}
            </StyledTableBodyRow>
        );
    }

    function PrintTitleRow(titleStringArray) {
        
        return (<StyledTableHeadRow  >{titleStringArray.map(columnName => <StyledTableHeadCell style={{zIndex:0}} key={uuidv4()}>{columnName}</StyledTableHeadCell>)}</StyledTableHeadRow>);
    }

    function PrintObjectPanel(panelObject, colSpan) {

        const entries = Object.entries(panelObject);
        
        return !!panelObject ? (
            <StyledTableBodyRow key={uuidv4()}>
                <StyledTableBodyCell colSpan={colSpan} style={{borderBottomWidth:"0px"}}>
                    <StyledTable>
                        <tbody>
                        
                            {
                                //* add controls to the top
                                (hasMenu && dataType === DataTypes.Object) ? (<StyledTableBodyCell  style={{borderBottomWidth:"0px"}}><div style={{display: "flex"}}>
                                    {CreateViewButton(propertyKey, entityMenus, panelObject)}
                                    {CreateTertiaryActionsMenu(propertyKey, entityMenus, panelObject)}
                                </div></StyledTableBodyCell>) : null
                            }
                            
                        {entries.map((kvPair, i) => {
                            const isLastRow = i === entries.length-1;
                            const [key, value] = kvPair;
                            
                            return  (
                                <StyledTableBodyRow key={uuidv4()}>
                                    {isRoot && dataType === DataTypes.Object ? null : <StyledTableBodyCell  style={isLastRow ? {borderBottomWidth:"0px"} : {}}>
                                    {ConvertObjectKeyToLabel(key)}
                                </StyledTableBodyCell>}
                                    <StyledTableBodyCell style={isLastRow ? {borderBottomWidth:"0px"} : {}}>
                                    {HandleProperty(key, value)}
                                </StyledTableBodyCell>
                            </StyledTableBodyRow>);
                        })}
                        </tbody>
                    </StyledTable>
                </StyledTableBodyCell>
            </StyledTableBodyRow>
        ) : null;
    }

    function HandleProperty(propertyKey, propertyValue) {

        const blobMetaMarkerGuid = "20fb62ff-9dd3-436e-a356-eceb335c2572";
        
        if (propertyRenderer &&
            propertyRenderer[propertyKey]) {
            return propertyRenderer[propertyKey]();
        } else {
            if (propertyValue instanceof Array) {
                
                return <ArrayRenderer propertyKey={propertyKey + "-Items"}
                                      arrayOfObjects={propertyValue}
                                      propertyRenderer={propertyRenderer}
                                      entityMenus={entityMenus}
                                      hiddenFields={hiddenFields}
                                      expandedFields={expandedFields}
                                      expandedFieldsFirstObjectOnly={expandedFieldsFirstObjectOnly}
                                      headerColumns={headerColumns}
                                      dataType={getArrayDataType(propertyValue)}
                />;
            } else if (IsChildObject(propertyValue)) {
                return <ArrayRenderer propertyKey={propertyKey}
                                      arrayOfObjects={[propertyValue]}
                                      propertyRenderer={propertyRenderer}
                                      entityMenus={entityMenus}
                                      hiddenFields={hiddenFields}
                                      expandedFields={expandedFields}
                                      expandedFieldsFirstObjectOnly={expandedFieldsFirstObjectOnly}
                                      headerColumns={headerColumns}
                                      dataType={DataTypes.Object}
                />;
            } else {
                let value;
                if (typeof propertyValue === typeof '') {
                    //string
                    if (propertyValue.includes("<")) {
                        let clean = DOMPurify.sanitize(propertyValue, {USE_PROFILES: {html: true}});
                        value = <div dangerouslySetInnerHTML={{__html: clean}}/>;
                    } else {
                        value = <span style={{whiteSpace: "pre-wrap"}}>{propertyValue}</span>;
                    }
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
                return value;
            }
        }

        function isPrimitive(arg) {
            return typeof arg === 'boolean' || typeof arg === 'number' || typeof arg === 'string';
        }

        function getArrayDataType(arr) {
            if (arr.length === 0) return DataTypes.ObjectArray;
            return isPrimitive(arr[0]) ? DataTypes.PrimitiveArray : DataTypes.ObjectArray;
        }

        function IsChildObject(propertyValue) {
            return typeof propertyValue === typeof {} &&
                typeof propertyValue !== typeof '' &&
                propertyValue !== null &&
                propertyValue.blobMetaMarker !== blobMetaMarkerGuid;
        }
    }

    function ConvertObjectKeyToLabel(key) {

        return Array.from(key.substring(key.indexOf("_") + 1)).map(z => convertPascalToPhrase(z));

        function convertPascalToPhrase(pascal) {
            let result = pascal.replace(/([A-Z])/g, " $1");
            result = result.charAt(0).toUpperCase() + result.slice(1);
            result = result.replace(/\d+$/, "");
            return result;
        }
    }

}


