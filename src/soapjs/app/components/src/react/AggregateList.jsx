import {Cell, Grid} from "baseui/layout-grid";
import {H5} from "baseui/typography";
import React from "react";
import {Button, KIND, SIZE} from "baseui/button";
import bus from "../soap/bus";
import {ArrayTableTop, EntityMenu} from "./Tables";
import {CenterSpinner} from "./CenterSpinner";
import {optional, types, validateArgs} from "../soap/util";

export function AggregateList(props) {

    const {title, entityMenus, aggregates, propertyRenderer, hiddenFields=[], expandedFields=[], refreshFunction, backFunction} = props;
    
    validateArgs(
        [{title}, types.string], 
        [{entityMenus}, types.object, optional],
        [{propertyRenderer}, types.object, optional],
        [{hiddenFields}, [types.string], optional],
        [{expandedFields}, [types.string], optional],
        [{refreshFunction}, types.function, optional],
        [{backFunction}, types.function, optional]
    );
    
    return (
        <Grid gridMaxWidth={1900}>
            <Cell span={12}>
                <H5>{title}
                    {backFunction ?
                        <Button overrides={{
                            BaseButton: {
                                style: ({$theme}) => ({
                                    marginLeft: "10px"
                                })
                            }
                        }} kind={KIND.minimal} onClick={() => backFunction()}>Back</Button> : null}
                    {refreshFunction ? <Button overrides={{
                        BaseButton: {
                            style: ({$theme}) => ({
                                marginLeft: "10px"
                            })
                        }
                    }} kind={KIND.secondary} size={SIZE.compact} onClick={() => {
                        bus.closeAllDialogs();
                        refreshFunction();
                    }}>Refresh List</Button> : null}
                </H5>
            </Cell>
            <Cell span={12}>
                {aggregates ?
                    <ArrayTableTop entityMenus={entityMenus} arrayOfObjects={aggregates} hiddenFields={hiddenFields} expandedFields={expandedFields} propertyRenderer={propertyRenderer}/>
                    :
                    <CenterSpinner/>}
            </Cell>
        </Grid>
    );
}