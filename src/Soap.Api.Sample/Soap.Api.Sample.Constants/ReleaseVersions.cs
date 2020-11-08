namespace Soap.Api.Sample.Constants
{

}

/*
 * ensure all values on messages are one of these types
 * 
 * string
 * bool
 * number
 * undefined (null)
 * custom object
 * datetime => needs adding
 * keyvaluepair
 *
 *
 * toasterEvent ->
 * toaster
 *
 * the following are all arranged at a page level according to a css3 gridlayout
 * toaster events for api errors handle at global level
 * toaster events for form val error handled inside formControl
 * can be handled in a js callback handle(
 *
 * panel
 * react router navLinks -> goto new page
 * 
 UIStates set/getUIState(key, value)
 List of UI states somewhere, you can use this to say what people are doing, to start chains and abandon things, only one active state is allowed at a time or it starts to get confusing
 more state can be stored int he database for sure, but there is only one key here. this way when you goto a page you can retrieve this state and show something specific based on the key and/or value
 *
 * UI Commands
 * sendGetFormQuery(anyparams) ->
 * get formDataEvent which has
 *     fields with these values
 *     commandfieldname
 *     initialvalue (could be a default value)
 *     selectablevalues
 *     fieldlabel
 *     a validate method - return bool
 *     a commitmethod - sends command
 *     a cancel method
 * -> put formDataEvent.data as prop into a form render control, add prop for onCommit/onCancel(optional) -> nav to new page
 *
 * sendGetViewQuery(anyparams)->
 * get viewDataEvent which have
 *     fieldLabel
 *     fieldValue
 * -> send viewDataEvent.data as prop into view render control
 *
 * sendGetTableQuery(anyparams)->
 * get listViewEvent which has  
 *     array of [viewDataEvent.data]
 *     array of [formDataEvent.data]
 *-> put into a list render control(panel + links) -> item action nav links with id param
 */
