import {useQuery} from "@soap/modules";
import {H5, Paragraph1} from "baseui/typography";
import React, {Fragment} from "react";
import {StyledLink} from "baseui/link";
import {ListItem} from "baseui/list";
import {StyledSpinnerNext} from 'baseui/spinner';

export function RecentlyAddedTestItems() {
    
    let e105 = useQuery({
        query: {
            $type: "Soap.Api.Sample.Messages.Commands.C111v1_GetRecentTestData"
        }
    });

    let listitems;
    if (e105) {
        listitems = e105.e105_TestData.map(x => (
            <ListItem key={x.e105_Id}>
                <Paragraph1>{x.e105_Label} created: {x.e105_CreatedAt}</Paragraph1>
                <StyledLink href={"#/test-data/view/" + x.e105_Id}>View</StyledLink>
            </ListItem>));
    } else {
        listitems = <StyledSpinnerNext/>;
    }

    return (
        <Fragment>
            <H5>Recently Added Test Items</H5>
            {listitems}
        </Fragment>
    );
}
