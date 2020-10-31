import { wireErrorHandlerOfLastResort } from '../index.js';

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
        window.onerror.call(window, e.toString());
    }


});