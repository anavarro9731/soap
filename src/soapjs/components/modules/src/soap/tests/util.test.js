import {optional, types, validateArgs} from '../util'

test("require args test", () => {

    //* pass
    testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], {
        o1: 1,
        o2: "two"
    }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    
    //- fail stringArg
    expect(() => {
        testArgs(null, 1, new Date(), new classOne(), [new classOne(), new classOne()], {
            o1: 1,
            o2: "two"
        }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- fail numberArg
    expect(() => {
        testArgs("test", null, new Date(), new classOne(), [new classOne(), new classOne()], {
            o1: 1,
            o2: "two"
        }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- fail dateArg
    expect(() => {
        testArgs("test", 1, null, new classOne(), [new classOne(), new classOne()], {
            o1: 1,
            o2: "two"
        }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- fail classOneArg
    expect(() => {
        testArgs("test", 1, new Date(), null, [new classOne(), new classOne()], {
            o1: 1,
            o2: "two"
        }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- fail classOneArrayArg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), null], {
            o1: 1,
            o2: "two"
        }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- pass empty array for classOneArrayArg
    testArgs("test", 1, new Date(), new classOne(), [], {
        o1: 1,
        o2: "two"
    }, undefined, "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    
    //- fail optionsArg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN, undefined,
            "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a"); //use NaN cause null is an object in JS
    }).toThrow();
    
    //- fail optional Arg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN, null,
            "2020-11-26T15:18:51.725Z", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- fail dateString Arg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN, undefined,
            "not-a-date", "7fd721fb-9486-4db5-825a-b4b43270010a");
    }).toThrow();
    
    //- fail guidString Arg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN, undefined,
            "2020-11-26T15:18:51.725Z", "not-a-guid");
    }).toThrow();

});

class classOne {

}

function testArgs(stringArg, numberArg, dateArg, classOneArg, classOneArrayArg, optionsArg, optionalArg, dateStringArg, guidStringArg) {

    validateArgs(
        [{stringArg}, types.string],
        [{numberArg}, types.number],
        [{dateArg}, Date],
        [{classOneArg}, classOne],
        [{classOneArrayArg}, [classOne]],
        [{optionsArg}, Object],
        [{optionalArg}, types.string, optional],
        [{dateStringArg}, types.datetime],
        [{guidStringArg}, types.guid]
    );
}
