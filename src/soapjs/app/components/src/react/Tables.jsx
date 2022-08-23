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
import {type} from "os-browserify/browser";

export function ArrayTable(props) {

    const [expandedRow, setExpandedRow] = useState();
    const {
        propertyKey,
        arrayOfObjects,
        propertyRenderer,
        entityMenus,
        hiddenFields,
        expandedFields,
        expandedFieldsFirstObjectOnly,
        headerColumns
    } = props;

    useEffect(() => {
        setExpandedRow(undefined);
    }, [arrayOfObjects]);

    const childrenPropertyKey = propertyKey + "-ArrayItems";
    const childrenHaveMenu = entityMenus && entityMenus[childrenPropertyKey];

    if (arrayOfObjects.length > 0) {

        const rows = [];
        const firstObjectKeys = Object.keys(arrayOfObjects[0]);

        //* BUILD HEADER TITLE
                
        const titleColumns = [...headerColumns.filter(x => firstObjectKeys.includes(x))]; //* first key of first object is always the title; 
        
        if (titleColumns.length === 0) titleColumns.push("$type");
        
        let titleComponentArray = ["CTRL"];
        for (const titleColumn of titleColumns) { //* should respect order
            if (firstObjectKeys.includes(titleColumn)) { //* read property names from first object and use to create title
                titleComponentArray.push(ConvertObjectKeyToLabel(titleColumn)); //* convert text after _ to Title Cased Phrase
            }
        }
             
        if (childrenHaveMenu) titleComponentArray.push("CTRL2"); //* add column for controls


        
        //* BUILD ROW PER OBJECT
        
        for (const [index, obj] of arrayOfObjects.entries()) {  //* add a row for each object
                        
            const rowObject = {
                rowIndex: index,
                rowHeaderComponentArray: [],
                expandableRowObject: null,
                isBaseTypeArray : false
            };
            
            if (typeof obj === typeof '') { //TODO or support other base types
                rowObject.rowHeaderComponentArray.push(obj);
                rowObject.isBaseTypeArray = true;
            } else {
                //* BUILD ROW HEADER
                for (const titleColumn of titleColumns) { //* add a column to row for each header
                    rowObject.rowHeaderComponentArray.push(HandleProperty(titleColumn, obj[titleColumn])); //* add data to the column
                }

                //* BUILD OBJECT PANEL
                const hiddenFieldsEx = [...hiddenFields, "$type", "headers", "validate", "types",]; //validate, types, headers would be present if the root event was passed in
                const objEntries = Object.entries(obj).filter(([key, _]) => !hiddenFieldsEx.includes(key)); //* select the fields not excluded from each object
                if (objEntries.length > 0) rowObject.expandableRowObject = Object.fromEntries(objEntries);
            }
            
            rows.push(rowObject);
        }

        //* PRINT TO SCREEN
        const isTop = propertyKey === "root";
        if (propertyKey === "e105_Countries-ArrayItems") {
            console.log("a",titleComponentArray, "b",titleColumns, "c",rows, "d",arrayOfObjects);
        }
        return PrintRows(rows, titleComponentArray, isTop);

    } else return  null;

    function PrintRows(rows, titleComponentArray, isTop) {
        
        const colSpan = titleComponentArray.length;
        
        return (<StyledTable>
                <thead>
                {rows.length > 1 && !rows[0].isBaseTypeArray? PrintTitleRow(titleComponentArray) : null}
                </thead>
                <tbody>
                {rows.map((r, i) => {
                    if (rows.length === 1 && !r.isBaseTypeArray) {
                        return PrintObjectPanel(r.expandableRowObject, colSpan, isTop);
                    } else
                    return expandedRow === i
                            ? [PrintHeaderRow(r), PrintObjectPanel(r.expandableRowObject, colSpan, isTop)]
                        : PrintHeaderRow(r)
                })}
                </tbody>
            </StyledTable>
        );
        
    }
    
    function PrintHeaderRow(rowObject) {

        const {rowHeaderComponentArray, expandableRowObject, rowIndex, isBaseTypeArray} = rowObject;
        
        if (childrenHaveMenu) rowHeaderComponentArray.push(<div style={{display: "flex"}}>
            {CreateViewButton(childrenPropertyKey, entityMenus, expandableRowObject)}
            {CreateTertiaryActionsMenu(childrenPropertyKey, entityMenus, expandableRowObject)}
        </div>);

        return (
            
            <StyledTableBodyRow key={uuidv4()}>
                {isBaseTypeArray === true ? null :
                <StyledTableBodyCell>
                    {expandedRow === rowIndex ?
                    <Button size={SIZE.mini} kind={KIND.secondary} shape={SHAPE.circle} onClick={() => setExpandedRow(undefined)}>-</Button> :
                    <Button size={SIZE.mini} kind={KIND.secondary} shape={SHAPE.circle} onClick={() => setExpandedRow(rowIndex)}>+</Button>}
                </StyledTableBodyCell>}
                {rowHeaderComponentArray.map(component => <StyledTableBodyCell key={uuidv4()}>{component}</StyledTableBodyCell>)}
            </StyledTableBodyRow>
        );
    }

    function PrintTitleRow(titleStringArray) {
        return (<StyledTableHeadRow>{titleStringArray.map(columnName => <StyledTableHeadCell key={uuidv4()}>{columnName}</StyledTableHeadCell>)}</StyledTableHeadRow>);
    }

    function PrintObjectPanel(panelObject, colSpan, isTop) {
        
        return !!panelObject ? (
            <StyledTableBodyRow key={uuidv4()}>
                <StyledTableBodyCell colSpan={colSpan}>
                    <StyledTable>
                        <tbody>
                        {Object.entries(panelObject).map(kvPair => {
                            
                           const [key, value] = kvPair;
                             
                           return (key !== "$type" ? (<StyledTableBodyRow key={uuidv4()}>
                               {isTop ? null :
                                <StyledTableBodyCell>
                                    {ConvertObjectKeyToLabel(key)} 
                                </StyledTableBodyCell>}
                               <StyledTableBodyCell>
                                   {HandleProperty(key, value)}
                               </StyledTableBodyCell>
                            </StyledTableBodyRow>) : null);
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
                return <ArrayTable propertyKey={propertyKey + "-ArrayItems"}
                                   arrayOfObjects={propertyValue}
                                   propertyRenderer={propertyRenderer}
                                   entityMenus={entityMenus}
                                   hiddenFields={hiddenFields}
                                   expandedFields={expandedFields}
                                   expandedFieldsFirstObjectOnly={expandedFieldsFirstObjectOnly}
                                   headerColumns={headerColumns}/>;
            } else if (IsChildObject(propertyValue)) {
                return <StyledTable><tbody>{PrintObjectPanel(propertyValue)}</tbody></StyledTable>;
            } else {
                let value;
                if (typeof propertyValue === typeof '') {
                    //string
                    
                    if (propertyValue.includes("<")) {
                        if (propertyValue.includes("<td>")) {
                            propertyValue = propertyValue.replaceAll("<td>", `<td style="border:1px solid black; white-space: pre;">`);
                        }
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
            const result = pascal.replace(/([A-Z])/g, " $1");
            return result.charAt(0).toUpperCase() + result.slice(1);
        }
    }



}


