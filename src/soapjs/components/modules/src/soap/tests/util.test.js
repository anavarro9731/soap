import { types, optional, validateArgs } from '../util'

test("require args test", () => {


    testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], { o1: 1, o2: "two" });
    //- fail stringArg
    expect(() => {
        testArgs(null, 1, new Date(), new classOne(), [new classOne(), new classOne()], { o1: 1, o2: "two" });
    }).toThrow();
    //- fail numberArg
    expect(() => {
        testArgs("test", null, new Date(), new classOne(), [new classOne(), new classOne()], { o1: 1, o2: "two" });
    }).toThrow();
    //- fail dateArg
    expect(() => {
        testArgs("test", 1, null, new classOne(), [new classOne(), new classOne()], { o1: 1, o2: "two" });
    }).toThrow();
    //- fail classOneArg
    expect(() => {
        testArgs("test", 1, new Date(), null, [new classOne(), new classOne()], { o1: 1, o2: "two" });
    }).toThrow();
    //- fail classOneArrayArg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), null], { o1: 1, o2: "two" });
    }).toThrow();
    //- fail optionsArg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN); //use NaN cause null is an object in JS
    }).toThrow();
    //- fail optional Arg
    expect(() => {
        testArgs("test", 1, new Date(), new classOne(), [new classOne(), new classOne()], NaN, null);
    }).toThrow();
});

class classOne {

}

function testArgs(stringArg, numberArg, dateArg, classOneArg, classOneArrayArg, optionsArg, optionalArg) {

    validateArgs(
        [{ stringArg }, types.string],
        [{ numberArg }, types.number],
        [{ dateArg }, Date],
        [{ classOneArg }, classOne],
        [{ classOneArrayArg }, [classOne]],
        [{ optionsArg }, Object],
        [{ optionalArg }, types.string, optional]
    );
}