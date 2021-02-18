import React from 'react';
import {StyledLink} from "baseui/link";
import {Route, Switch, Link} from "react-router-dom";
import {ViewTestData} from "./ViewTestData";
import {CreateTestData} from "./CreateTestData";
import {EditTestData} from "./EditTestData";
import {RecentlyAddedTestItems} from "./RecentlyAddedTestItems";
import {useStyletron} from 'baseui';
import {ProtectedRoute} from "@soap/modules";

export default function() {
    const [css, theme] = useStyletron();
    return (
            <Switch>
                <ProtectedRoute path="/test-data/new" component={CreateTestData} />
                <ProtectedRoute path="/test-data/edit/:id" component={EditTestData} />
                
                <ProtectedRoute path="/test-data/view/:id" component={ViewTestData}/>                 
                
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
    );
}
