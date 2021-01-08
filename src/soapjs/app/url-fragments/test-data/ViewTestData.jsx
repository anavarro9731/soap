import {JsonView} from "@soap/modules";
import {H1} from "baseui/typography";
import React, {Fragment} from "react";
import {useParams} from "react-router-dom";
import {StyledLink} from "baseui/link"

export function ViewTestData() {
    
    const { id } = useParams();
    
    return (
        <Fragment>
            <H1>View</H1>
            <JsonView query={{
                $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
                c110_TestDataId: id
            }} />
        </Fragment>
    );
}
