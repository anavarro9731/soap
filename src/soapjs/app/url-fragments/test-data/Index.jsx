import React from 'react';
import {Route, Switch} from "react-router-dom";
import {ViewTestData} from "./ViewTestData";
import {ListTestData} from "./ListTestData";
import {ProtectedRoute} from "@soap/modules";

export default function () {

    return (
        <Switch>
            <ProtectedRoute path="/test-data/view/:id" component={ViewTestData}/>

            <Route path="/test-data/">
                <ListTestData/>
            </Route>
        </Switch>
    );
}
