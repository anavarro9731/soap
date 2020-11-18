import queryCache from "../query-cache";

import * as __ from '../util'
import messageDefinitions, {TestCommand_c100v1} from "./test-messages";
import {createRegisteredTypedMessageInstanceFromAnonymousObject, registerMessageTypes} from "../messages";

test("queries filtered results1", () => {

    function Cache(code) {
        const hash = __.md5Hash({id : code});
        queryCache.addOrReplace(hash, {id: code});
    }
    
    Cache("abc");
    Cache("def");
    Cache("ghi");
    
    let result = queryCache.find({id : 'def'}, 10);
    expect(result).toEqual({id:"def"});
});

test("queries filtered results2", () => {

    registerMessageTypes(messageDefinitions);
    
    
    
    function Cache(code) {
        const hash = __.md5Hash({id : code});
        queryCache.addOrReplace(hash, {id: code});
    }

    Cache("abc");
    Cache("def");
    Cache("ghi");

    let result = queryCache.find({id : 'zyx'}, 10);

    expect(result).toBeUndefined();
});