import React from 'react';
import { Table } from "baseui/table-semantic";
import {StatefulPanel} from "baseui/accordion";
import {Button} from "baseui/button";
import {ConvertObjectToComponentArray} from "./fieldFormatters";

export function ObjectTableNested(props) {
    const {object, propertyRenderer} = props;
    return(<StatefulPanel>
            <div>
                <Button>Actions</Button>
                <Button>View</Button>
            </div>
            <div>
                <TableWithoutHeadersWithBorder data={getObjectTableData(object,propertyRenderer)} />
            </div>
        </StatefulPanel>
    )
}

export function ObjectTableTop(props) {
    const {object, propertyRenderer} = props;
    
    return(
        <React.Fragment>
            <div>
                <Button>Actions</Button>
                <Button>View</Button>
            </div>
            <div>
                <TableWithoutHeadersOrBorder data={getObjectTableData(object, propertyRenderer)} />
            </div>
        </React.Fragment>
    );
}

function getObjectTableData(object, propertyRenderer) {
    const labels = Object.keys(object).filter(nameOfProperty => nameOfProperty !== "validate" && nameOfProperty !== "types" && nameOfProperty !== "$type")
        .map(k => k.substring(k.indexOf("_") + 1));
    const arrayOfComponentsFromObjectProperties = ConvertObjectToComponentArray(object, propertyRenderer);
    return arrayOfComponentsFromObjectProperties.map((item,index) => [labels[index], item]);
}

export function ArrayTableNested(props) {
    const {arrayOfObjects, propertyRenderer} = props;
    return(<StatefulPanel>
            <div>
                <Button>Actions</Button>
                <Button>View</Button>
            </div>
            <div>
                <ArrayTable arrayOfObjects={arrayOfObjects} propertyRenderer={propertyRenderer} />
            </div>
        </StatefulPanel>
    )
}

export function ArrayTableTop(props) {
    const {arrayOfObjects, propertyRenderer} = props;
    return(<React.Fragment>
            <div>
                <Button>Actions</Button>
                <Button>View</Button>
            </div>
            <div>
                <ArrayTable arrayOfObjects={arrayOfObjects} propertyRenderer={propertyRenderer} />
            </div>
        </React.Fragment>
    )
}

function ArrayTable(props) {
    const {arrayOfObjects, propertyRenderer} = props;
    
    const headers = [];
    if (arrayOfObjects.length > 0) {
        headers.push(...Object.keys(arrayOfObjects[0]).filter(x => x !== "$type").map(k => k.substring(k.indexOf("_") + 1)));
    }
    
    const arrayOfHorizontalTableControlArrays = [];
    for (const obj of arrayOfObjects) {
        arrayOfHorizontalTableControlArrays.push(ConvertObjectToComponentArray(obj, propertyRenderer));
    }
    
    return(
        <TableWithHeadersAndBorder data={arrayOfHorizontalTableControlArrays} headers={headers} />
    );
}

function TableWithoutHeadersOrBorder(props) {
    const {data} = props;
    return (<Table
        $style={{
            borderRadius: "0px",
            border: "0px"
        }}
        overrides={{
            Table: {
                style: {
                    minWidth: "min-content"
                }
            },
            TableHeadCell: {
                style: ({ $theme }) => ({
                    ":after": {
                        backgroundImage: "none"
                    },
                    ":before": {
                        border: "0px"
                    },
                    padding:"0px"
                })
            }
        }}
        columns={[null,null]}
        data={data}
        emptyMessage="No items"
    />);    
}

function TableWithoutHeadersWithBorder(props) {
    const {data} = props;
    const columns = data.length;
    const headers = new Array(columns);
    return (
        <Table
            $style={{
                borderRadius: "0px",
            }}
            overrides={{
                Table: {
                    style: {
                        minWidth: "min-content"
                    }
                },
                TableHeadCell: {
                    style: ({ $theme }) => ({
                        ":after": {
                            backgroundImage: "none"
                        },
                        ":before": {
                            border: "0px"
                        },
                        padding:"5px"
                    })
                }
            }}
            columns={headers}
            data={data}
            emptyMessage="No items"
        />);
}

function TableWithHeadersAndBorder(props) {
    const {data, headers} = props;
    return (
        <Table
        $style={{
            borderRadius: "0px",
        }}
        overrides={{
            Table: {
                style: {
                    minWidth: "min-content"
                }
            },
            TableHeadCell: {
                style: ({ $theme }) => ({
                    ":after": {
                        backgroundImage: "none"
                    },
                    ":before": {
                        border: "0px"
                    },
                    padding:"5px"
                })
            }
        }}
        columns={headers}
        data={data}
        emptyMessage="No items"
    />);
}
