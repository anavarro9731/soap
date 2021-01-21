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

    let listItems;
    if (e105) {
        listItems = e105.e105_TestData.map(item => (
            <ListItem key={item.e105_Id}>
                <Paragraph1>{item.e105_Label} created: {item.e105_CreatedAt}</Paragraph1>
                <StyledLink href={"#/test-data/view/" + item.e105_Id}>View</StyledLink>
            </ListItem>
        ));
    } else {
        listItems = <StyledSpinnerNext/>;
    }

    return (
        <Fragment>
            <H5>Recently Added Test Items</H5>
            {listItems}
        </Fragment>
    );
}
