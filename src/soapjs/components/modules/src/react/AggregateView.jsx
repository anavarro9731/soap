import {Cell, Grid} from "baseui/layout-grid";
import React from "react";
import {Button, KIND} from "baseui/button";
import bus from "../soap/bus";
import {Heading, HeadingLevel} from "baseui/heading";
import {ObjectTableTop} from "./Tables";
import {CenterSpinner} from "./CenterSpinner";

export function AggregateView(props) {

    const {title, aggregate, propertyRenderer, refreshFunction, backFunction} = props;

    return (
        <Grid gridMaxWidth={1600}>
            <Cell span={12}>
                <HeadingLevel><Heading>{title}
                    {backFunction ?
                        <Button  kind={KIND.minimal} overrides={{
                            BaseButton: {
                                style: ({$theme}) => ({
                                    marginLeft: "10px"
                                })
                            }
                        }} onClick={() => backFunction()}>Back</Button> : null}
                    {refreshFunction ? <Button kind={KIND.minimal} overrides={{
                        BaseButton: {
                            style: ({$theme}) => ({
                                marginLeft: "10px"
                            })
                        }
                    }} onClick={() => {
                        bus.closeAllDialogs();
                        refreshFunction();
                    }}>Refresh</Button> : null}
                </Heading></HeadingLevel>
            </Cell>
            <Cell span={12}>
                {aggregate ?
                    <ObjectTableTop object={aggregate} propertyRenderer={propertyRenderer}/>
                    :
                    <CenterSpinner/>}
            </Cell>
        </Grid>
    );
}