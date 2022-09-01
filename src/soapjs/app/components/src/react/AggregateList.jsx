import {Cell, Grid} from "baseui/layout-grid";
import {H5} from "baseui/typography";
import React from "react";
import {Button, KIND, SIZE} from "baseui/button";
import bus from "../soap/bus";
import {ArrayRenderer} from "./ArrayRenderer";
import {CenterSpinner} from "./CenterSpinner";
import {optional, types, validateArgs} from "../soap/util";
import {CreatePrimaryActionsMenu} from "./ActionMenu";


export function AggregateView(props) {
    
    const { expandedFieldsFirstObjectOnly = [] } = props; 
    const newProps = { expandedFieldsFirstObjectOnly : ["root", ...expandedFieldsFirstObjectOnly], aggregates : [props.aggregate],  ...props, dataType: "object" };
    delete newProps.aggregate;
    //* console.log(newProps);
    return AggregateList(newProps);
}

export function AggregateList(props) {

    const {
        title,
        entityMenus,
        aggregates,
        propertyRenderer,
        hiddenFields = [],
        expandedFields = [],
        expandedFieldsFirstObjectOnly = [],
        headerColumns = [],
        refreshFunction,
        backFunction,
        dataType
    } = props;

    validateArgs(
        [{entityMenus}, types.object, optional],
        [{propertyRenderer}, types.object, optional],
        [{hiddenFields}, [types.string], optional],
        [{headerColumns}, [types.string], optional],
        [{expandedFields}, [types.string], optional],
        [{expandedFieldsFirstObjectOnly}, [types.string], optional],
        [{refreshFunction}, types.function, optional],
        [{backFunction}, types.function, optional]
    );

    function headerControls() {
        return (<>
            {backFunction ?
                <Button overrides={{
                    BaseButton: {
                        style: ({$theme}) => ({
                            marginLeft: "10px"
                        })
                    }
                }} kind={KIND.minimal} onClick={() =>
                    backFunction()
                }>Back</Button> : null}
            {refreshFunction ?
                <Button overrides={{
                    BaseButton: {
                        style: ({$theme}) => ({
                            marginLeft: "10px"
                        })
                    }
                }} kind={KIND.secondary} size={SIZE.compact} onClick={() => {
                    bus.closeAllDialogs();
                    refreshFunction();
                }}>Refresh List</Button> : null}</>);
    }

    return (
        <Grid gridMaxWidth={1200}>
            <Cell span={12}>
                {(typeof title === typeof "") ? (<H5>
                    {CreatePrimaryActionsMenu("root", entityMenus)}&nbsp;
                    {headerControls()}&nbsp;
                    {title}
                    
                </H5>) : <>{CreatePrimaryActionsMenu("root", entityMenus)}&nbsp;{headerControls()}</>}
            </Cell>
            <Cell span={12}>
                {aggregates ?
                        <ArrayRenderer entityMenus={entityMenus}
                                       arrayOfObjects={aggregates}
                                       hiddenFields={hiddenFields}
                                       headerColumns={headerColumns}
                                       expandedFields={expandedFields}
                                       expandedFieldsFirstObjectOnly={expandedFieldsFirstObjectOnly}
                                       propertyRenderer={propertyRenderer}
                                       propertyKey={"root-Items"}
                                       dataType={dataType}
                        />
                    :
                    <CenterSpinner/>}
            </Cell>
        </Grid>
    );
}