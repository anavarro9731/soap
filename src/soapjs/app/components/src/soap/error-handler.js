import bus from './bus'
import config from './config'

export default function wireErrorHandlerOfLastResort(finalGlobalCallback) {

    global.onerror = function (message, url, line, col, error) {
        // col & error are new to the HTML 5 spec and may not be 
        // supported in every browser.
        var extra = !col ? '' : '\ncolumn: ' + col;
        extra += !error ? '' : '\nerror: ' + error;

        bus.subscribe(bus.channels.errors, '#', finalGlobalCallback);

        //send the message to subscribers
        const complexError = { message, line, col, error };
        bus.publish(bus.channels.errors, "window", complexError);

        config.logger.log("** UNHANDLED ERROR: **\n" + complexError.message);

        const suppressErrorAlert = true;
        // If you return true, then any native browser error alerts will be suppressed.
        return suppressErrorAlert;
    };

}