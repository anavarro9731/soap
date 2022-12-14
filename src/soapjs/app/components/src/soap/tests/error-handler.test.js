import wireErrorHandlerOfLastResort from '../error-handler';

test("unhandled errors are caught", () => {
    
    //arrange
    wireErrorHandlerOfLastResort((error) => {
        //assert
        expect(error.message).toMatch(/Whoa!/g);
    });

    try {
        //act 
        throw new Error("Whoa!");
    } catch (e) {
        globalThis.onerror.call(globalThis, e.toString());
    }


});
