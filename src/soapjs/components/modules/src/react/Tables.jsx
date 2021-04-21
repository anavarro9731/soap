import React, {Fragment} from 'react';
import {Table} from "baseui/table-semantic";
import {StatefulPanel} from "baseui/accordion";
import {Button, KIND, SIZE} from "baseui/button";
import {optional, types, uuidv4, validateArgs} from "../soap/util";
import {SecondaryActionMenu, ViewMenu} from "./ActionMenu";
import {FileView} from "./FileView";
import {Label2, Label3} from "baseui/typography";


export class EntityMenu {
    constructor(viewAction, actions) {

        validateArgs(
             //viewAction > (entity) => action or [(entity) => component]
            [{actions}, [types.function], optional] //(entity) => {}
        );

        this.viewAction = viewAction;
        this.actions = actions;
    }
}

export function ObjectTableNested(props) {
    const {propertyKey, object, propertyRenderer, entityMenus} = props;

    return (<StatefulPanel overrides={{
            PanelContainer: {
                style: ({$theme}) => ({

                    backgroundColor: $theme.colors.backgroundPrimary,

                })
            },
            Content: {

                style: ({$theme}) => ({
                    backgroundColor: $theme.colors.backgroundPrimary,

                })
            }
        }}>
            <div>
                {CreateViewButton(propertyKey, entityMenus, object)}
                {CreateActionsMenu(propertyKey, entityMenus, object)}
            </div>
            <div>
                <TwoColumnTableWithoutHeadersWithBorder
                    data={getObjectTableData(object, propertyRenderer, entityMenus)}/>
            </div>
        </StatefulPanel>
    )
}

export function ObjectTableTop(props) {
    const {object, propertyRenderer, entityMenus} = props;
    
    return (
        <React.Fragment>
            <div>
                {CreateViewButton("root", entityMenus, object)}
                {CreateActionsMenu("root", entityMenus, object)}
            </div>
            <div>
                <TwoColumnTableWithoutHeadersOrBorder data={getObjectTableData(object, propertyRenderer, entityMenus)}/>
            </div>
        </React.Fragment>
    );
}


export function ArrayTableNested(props) {
    const {propertyKey, arrayOfObjects, propertyRenderer, entityMenus} = props;
    return (<StatefulPanel overrides={{
            PanelContainer: {
                style: ({$theme}) => ({

                    backgroundColor: $theme.colors.backgroundPrimary,

                })
            },
            Content: {

                style: ({$theme}) => ({
                    backgroundColor: $theme.colors.backgroundPrimary,

                })
            }
        }}>
            <div>
                {CreateActionsMenu(propertyKey, entityMenus)}
            </div>
            <div>
                <ArrayTable propertyKey={propertyKey} arrayOfObjects={arrayOfObjects}
                            propertyRenderer={propertyRenderer} entityMenus={entityMenus}/>
            </div>
        </StatefulPanel>
    )
}

export function ArrayTableTop(props) {
    const {arrayOfObjects, propertyRenderer, entityMenus} = props;
    return (<React.Fragment>
            <div>
                {CreateViewButton("root", entityMenus)}
                {CreateActionsMenu("root", entityMenus)}
            </div>
            <div>
                <ArrayTable propertyKey={"root"} arrayOfObjects={arrayOfObjects} propertyRenderer={propertyRenderer}
                            entityMenus={entityMenus}/>
            </div>
        </React.Fragment>
    )
}

function CreateActionsMenu(propertyKey, entityMenus, entity) {

    validateArgs(
        [{propertyKey}, types.string],
        [{entityMenus}, types.object, optional],
        [{entity}, types.object, optional]
    );
    if (entityMenus && entityMenus[propertyKey]?.actions) {
        const actions = entityMenus[propertyKey]?.actions;

        return (<SecondaryActionMenu>
            {actions.map(action => <div key={uuidv4()}>{action(entity)}</div>)}
        </SecondaryActionMenu>);
    } else {
        return null;
    }
}

function CreateViewButton(propertyKey, entityMenus, entity) {

    validateArgs(
        [{propertyKey}, types.string],
        [{entityMenus}, types.object, optional],
        [{entity}, types.object, optional]
    );

    if (entityMenus && entityMenus[propertyKey]?.viewAction) {
        const viewAction = entityMenus[propertyKey]?.viewAction;
        
        if (viewAction instanceof Array) {
            return (<ViewMenu>
                {viewAction.map(action => <div key={uuidv4()}>{action(entity)}</div>)}
            </ViewMenu>);
        } else {
            return (<Button kind={KIND.secondary} size={SIZE.compact}
                                      onClick={() => viewAction(entity)}>View</Button>);    
        }
    } else {
        return null;
    }
}

function getObjectTableData(object, propertyRenderer, entityMenus) {

    const labels = Object.keys(object).filter(nameOfProperty => nameOfProperty !== "validate" && nameOfProperty !== "types" && nameOfProperty !== "$type")
        .map(k => k.substring(k.indexOf("_") + 1)).map(l => <Label3>{l}</Label3>);
    const arrayOfComponentsFromObjectProperties = ConvertObjectToComponentArray(object, propertyRenderer, entityMenus);
    const arrayWithLabels = arrayOfComponentsFromObjectProperties.map((item, index) => [labels[index], item]);

    return arrayWithLabels;
}


function ArrayTable(props) {

    const {propertyKey, arrayOfObjects, propertyRenderer, entityMenus} = props;
    const headers = [];

    const childrenPropertyKey = propertyKey + "-ArrayItems";
    const childrenHaveMenu = entityMenus && entityMenus[childrenPropertyKey];

    if (arrayOfObjects.length > 0) {
        const firstObject = arrayOfObjects[0];
        if (typeof firstObject === typeof "") {
            headers.push("");
        } else {
            const keys = Object.keys(firstObject);
            headers.push(...Object.keys(arrayOfObjects[0]).filter(x => x !== "$type").map(k => k.substring(k.indexOf("_") + 1)));
            if (childrenHaveMenu) {
                headers.push(""); //* column header for actions
            }    
        }
    }

    const arrayOfHorizontalTableControlArrays = [];
    for (const obj of arrayOfObjects) {
        if (typeof obj === typeof "") {
            arrayOfHorizontalTableControlArrays.push([obj]);
        } else {
            const componentArray = ConvertObjectToComponentArray(obj, propertyRenderer, entityMenus);
            if (childrenHaveMenu) { //* actions column
                componentArray.push(<div>
                    {CreateViewButton(childrenPropertyKey, entityMenus, obj)}
                    {CreateActionsMenu(childrenPropertyKey, entityMenus, obj)}
                </div>)
            }
            arrayOfHorizontalTableControlArrays.push(componentArray);
        }
        
    }

    return (
        <TableWithHeadersAndBorder data={arrayOfHorizontalTableControlArrays} headers={headers}/>
    );
}


function TwoColumnTableWithoutHeadersOrBorder(props) {
    const {data} = props;
    return (<Table
        $style={{
            borderTopLeftRadius: "0px",
            borderTopRightRadius: "0px",
            borderBottomLeftRadius: "0px",
            borderBottomRightRadius: "0px",
            borderTopWidth: "0px",
            borderBottomWidth: "0px",
            borderLeftWidth: "0px",
            borderRightWidth: "0px"
        }}
        overrides={{
            Table: {
                style: {
                    minWidth: "min-content"
                }
            },
            TableHeadCell: {
                style: ({$theme}) => ({
                    ":after": {
                        backgroundImage: "none"
                    },
                    ":before": {
                        border: "0px"
                    },
                    paddingTop: "0px",
                    paddingBottom: "0px",
                    paddingRight: "0px",
                    paddingLeft: "0px"
                })
            }
        }}
        columns={[null, null]}
        data={data}
        emptyMessage="No items"
    />);
}

function TwoColumnTableWithoutHeadersWithBorder(props) {
    const {data} = props;

    return (
        <Table
            $style={{
                borderTopLeftRadius: "0px",
                borderTopRightRadius: "0px",
                borderBottomLeftRadius: "0px",
                borderBottomRightRadius: "0px",
            }}
            overrides={{
                Table: {
                    style: {
                        minWidth: "min-content"
                    }
                },
                TableHeadCell: {
                    style: ({$theme}) => ({
                        ":after": {
                            backgroundImage: "none"
                        },
                        ":before": {
                            border: "0px"
                        },
                        paddingTop: "0px",
                        paddingBottom: "0px",
                        paddingRight: "0px",
                        paddingLeft: "0px"
                    })
                }
            }}
            columns={[null, null]}
            data={data}
            emptyMessage="No items"
        />);
}

function TableWithHeadersAndBorder(props) {
    const {data, headers} = props;
    return (
        <Table
            $style={{
                borderTopLeftRadius: "0px",
                borderTopRightRadius: "0px",
                borderBottomLeftRadius: "0px",
                borderBottomRightRadius: "0px",
            }}
            overrides={{
                Table: {
                    style: {
                        minWidth: "min-content"
                    }
                },
                TableHeadCell: {
                    style: ({$theme}) => ({
                        ":after": {
                            backgroundImage: "none"
                        },
                        ":before": {
                            border: "0px"
                        },

                    })
                }
            }}
            columns={headers}
            data={data}
            emptyMessage="No items"
        />);
}


const blobMetaMarkerGuid = "20fb62ff-9dd3-436e-a356-eceb335c2572";

function ConvertObjectToComponentArray(object, propertyRenderer, entityMenus) {

    const arrayOfComponentsFromObjectProperties = [];

    for (const nameOfProperty in object) {
            if (nameOfProperty !== "validate" && nameOfProperty !== "types" && nameOfProperty !== "$type" && nameOfProperty !== "headers") { //* filter the ones i manually added

                const propertyValue = object[nameOfProperty];

                if (propertyValue instanceof Array) {
                    arrayOfComponentsFromObjectProperties.push(<ArrayTableNested propertyKey={nameOfProperty}
                                                                                 arrayOfObjects={propertyValue}
                                                                                 propertyRenderer={propertyRenderer}
                                                                                 entityMenus={entityMenus}/>);
                } else if (isChildObject(propertyValue)) {
                    arrayOfComponentsFromObjectProperties.push(<ObjectTableNested propertyKey={nameOfProperty}
                                                                                  object={propertyValue}
                                                                                  propertyRenderer={propertyRenderer}
                                                                                  entityMenus={entityMenus}/>)
                } else {
                    arrayOfComponentsFromObjectProperties.push(ConvertPropertyToComponent(nameOfProperty, propertyValue, propertyRenderer));
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
            return propertyRenderer[nameOfProperty](value);
        } else {
            return value;
        }
    }
}