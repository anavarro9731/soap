import * as React from "react";
import { Accordion, Panel } from "baseui/accordion";
import { Button, KIND, SIZE } from "baseui/button";
import ChevronDown from "baseui/icon/chevron-down";
import { styled } from "baseui";
import { withStyle } from "styletron-react";

export default (props) => {
    
    const { headers, rows, objects } = props;
    
    const AutoSpaceGrid = styled("div", ({ $theme }) => ({
        display: "grid",
        gridAutoFlow: "column",
        gap: "1em",
        gridAutoColumns: "1fr",
        minWidth: "95%",
        ...$theme.typography.LabelSmall,
        fontWeight:"normal"
    }));

    const Header = styled(AutoSpaceGrid, ({ $theme }) => ({
        minWidth: "95%"
    }));

    const HeaderColumn = styled(
        "div",
        ({ $theme }) => $theme.typography.HeadingXSmall
    );

    return (
        <>
            <Header>
                {headers.map(c => (<HeaderColumn>{c}</HeaderColumn>))}
            </Header>
            <Accordion
                onChange={({ expanded }) => console.log(expanded)}
                accordion
                overrides={{
                    ToggleIcon: {
                        component: () => (
                            <Button kind={KIND.tertiary} size={SIZE.mini}>
                                <ChevronDown />
                            </Button>
                        )
                    }
                }}
            >
                {rows.map(row => (
                <Panel
                    title={
                        <AutoSpaceGrid>
                            {row}
                        </AutoSpaceGrid>
                    }
                >
                    {objects.map(obj => (<>obj here</>))}
                </Panel>))}
            </Accordion>
        </>
    );
};
