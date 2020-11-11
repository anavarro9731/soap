import queryCache from "../query-cache";

import * as __ from '../util'
import {TestCommand_c100v1} from "./test-messages";

test("queries filtered results1", () => {

    function Cache(code) {
        const {headers, ...payload} = new TestCommand_c100v1({c100_pointlessProp: code});
        const hash = __.md5Hash(payload);
        queryCache.addOrReplace(hash, {id: code});
    }
    
    Cache("abc");
    Cache("def");
    Cache("ghi");
    
    let result = queryCache.find(new TestCommand_c100v1({c100_pointlessProp: "def"}), 10);

    expect(result).toEqual({id:"def"});
});

test("queries filtered results2", () => {

    function Cache(code) {
        const {headers, ...payload} = new TestCommand_c100v1({c100_pointlessProp: code});
        const hash = __.md5Hash(payload);
        queryCache.addOrReplace(hash, {id: code});
    }

    Cache("abc");
    Cache("def");
    Cache("ghi");

    let result = queryCache.find(new TestCommand_c100v1({c100_pointlessProp: "xyz"}), 10);

    expect(result).toBeUndefined();
});