import queryCache from "../query-cache";
import {ApiMessage} from "../messages";
import * as __ from '../util'
import {TestCommand_c100v1} from "./test-messages";

test("queries filtered results", () => {

    queryCache.addOrReplace(
        __.md5Hash(new TestCommand_c100v1({c100_pointlessProp: "2342342342"})),
        Object.assign(new ApiMessage())
    );

    queryCache.addOrReplace(
        __.md5Hash(new TestCommand_c100v1({c100_pointlessProp: "6788676576576"})),
        Object.assign(new ApiMessage())
    );

    queryCache.addOrReplace(
        __.md5Hash(new TestCommand_c100v1({c100_pointlessProp: "57567464564"})),
        Object.assign(new ApiMessage())
    );

    let result = queryCache.find(new TestCommand_c100v1({c100_pointlessProp: "2342342342"}), 10);

    expect(result).toBeInstanceOf(ApiMessage);
});

test("queries filtered results", () => {

    queryCache.addOrReplace(
        __.md5Hash(new TestCommand_c100v1({c100_pointlessProp: "2342342342"})),
        Object.assign(new ApiMessage())
    );

    queryCache.addOrReplace(
        __.md5Hash(new TestCommand_c100v1({c100_pointlessProp: "6788676576576"})),
        Object.assign(new ApiMessage())
    );

    queryCache.addOrReplace(
        __.md5Hash(new TestCommand_c100v1({c100_pointlessProp: "57567464564"})),
        Object.assign(new ApiMessage())
    );

    let result = queryCache.find(new TestCommand_c100v1({c100_pointlessProp: "6788676576576"}), 5);

    expect(result).toBeInstanceOf(ApiMessage);
});

