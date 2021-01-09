import React, {Fragment} from 'react';
import {StyledLink} from "baseui/link";
import {Route, Switch, Link} from "react-router-dom";
import {ViewTestData} from "./ViewTestData";
import {CreateTestData} from "./CreateTestData";
import {RecentlyAddedTestItems} from "./RecentlyAddedTestItems";
import {useStyletron} from 'baseui';


export default function() {
    const [css, theme] = useStyletron();
    return (
        <Fragment>
            <Switch>
                <Route path="/test-data/new">
                    <CreateTestData/>
                </Route>
                <Route path="/test-data/view/:id">
                    <ViewTestData/>
                </Route>
                <Route path="/test-data/">
                    <RecentlyAddedTestItems />
                    <div className={css({
                        'text-align':'right',
                        'padding': '10px'
                    })}>
                        <StyledLink href="#/test-data/new">
                            Add New Test Data
                        </StyledLink>
                    </div>
                </Route>
            </Switch>
        </Fragment>
    )
}
