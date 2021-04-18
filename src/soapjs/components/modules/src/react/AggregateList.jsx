import {Cell, Grid} from "baseui/layout-grid";
import {H5} from "baseui/typography";
import React from "react";
import {Button, KIND} from "baseui/button";
import bus from "../soap/bus";
import {ArrayTableTop} from "./Tables";
import {CenterSpinner} from "./CenterSpinner";

export function AggregateList(props) {

    const {title, aggregates, propertyRenderer, refreshFunction, backFunction} = props;

    return (
        <Grid gridMaxWidth={1600}>
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
                    }} kind={KIND.secondary} onClick={() => {
                        bus.closeAllDialogs();
                        refreshFunction();
                    }}>Refresh List</Button> : null}
                </H5>
            </Cell>
            <Cell span={12}>
                {aggregates ?
                    <ArrayTableTop arrayOfObjects={aggregates} propertyRenderer={propertyRenderer}/>
                    :
                    <CenterSpinner/>}
            </Cell>
        </Grid>
    );
}