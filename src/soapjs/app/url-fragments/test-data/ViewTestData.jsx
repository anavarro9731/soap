import {useQuery, FileView} from "@soap/modules";
import {H1, Label3} from "baseui/typography";
import React, {Fragment} from "react";
import {useParams} from "react-router-dom";
import {StyledLink} from "baseui/link";
import {Cell, Grid} from "baseui/layout-grid";
import {useStyletron, withStyle} from 'baseui';
import {StyledSpinnerNext} from "baseui/spinner";

export function ViewTestData() {

    const [css] = useStyletron();
    const { id } = useParams();
    
    const CenterSpinner = withStyle(StyledSpinnerNext, {
        margin: "auto"
    });

    const query={
        $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
        c110_TestDataId: id
    };

    const [dataViewEvent, refresh] = useQuery({query});
    
    if (dataViewEvent) {
        
        return (
            <React.Fragment>
                <Grid>
                    <Cell span={12}>
                        <div className={css({
                            marginTop: '40px',
                        })}>
                            <StyledLink href="#/test-data">
                                Back
                            </StyledLink>
                            <H1>{dataViewEvent.e102_TestData.e102_String}</H1>
                            
                        </div>
                    </Cell>
                </Grid>
                <Grid gridGaps={20}>
                    <Cell span={2}>
                        <Label3>Title</Label3>
                    </Cell>
                    <Cell span={2}>
                        <Label3>{dataViewEvent.e102_TestData.e102_Guid}</Label3>
                    </Cell>
                    <Cell span={8}>
                    </Cell>

                    <Cell span={2}>
                        <Label3>Image</Label3>
                    </Cell>
                    <Cell span={2}>
                        <FileView value={dataViewEvent.e102_TestData.e102_Image} />
                    </Cell>
                    <Cell span={8}>
                    </Cell>

                    <Cell span={2}>
                        <Label3>File</Label3>
                    </Cell>
                    <Cell span={2}>
                        <FileView value={dataViewEvent.e102_TestData.e102_File} />
                        
                    </Cell>
                    <Cell span={8}>
                    </Cell>
                </Grid>
            </React.Fragment>
        );
    } else {
        return <CenterSpinner />;
    }
}
