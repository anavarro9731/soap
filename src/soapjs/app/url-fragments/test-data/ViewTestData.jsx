import {JsonView} from "@soap/modules";
import {H1} from "baseui/typography";
import React, {Fragment} from "react";
import {useParams} from "react-router-dom";
import {StyledLink} from "baseui/link";
import {Cell, Grid} from "baseui/layout-grid";
import {useStyletron} from 'baseui';
export function ViewTestData() {

    const [css] = useStyletron();
    const { id } = useParams();
    
    return (
        <Grid >
            <Cell>
                <div className={css({
                    marginTop: '40px',
                })}>
            <StyledLink href="#/test-data">
                Back
            </StyledLink>
            <H1>View</H1>
            <JsonView query={{
                $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
                c110_TestDataId: id
            }} />
                </div>
            </Cell>
        </Grid>
    );
}
