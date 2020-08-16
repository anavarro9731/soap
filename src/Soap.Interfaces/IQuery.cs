namespace Soap.Interfaces
{
    public interface IQuery
    {
        /* was a call whether to share these between ApiCommands and also ApiQuery handlers
         in the end I opted to do so because to keep them separate inevitably means duplication 
        probably a lot of it and over time might introduce indirect bugs between what the user sees in a 
        query and makes decisions off of and what the business logic uses to process a command
        even though not affecting each other reduces bugs in some scenarios. On balance it seemed better
        this way. Either way these are not ApiQueries, so they are more infrastructural and any definition
        of a concept made implicitly through the query logic (e.g. GetHighestScore) will change
        everywhere at once if you change it here. That is a rule that needs to be considered every
        time. */
    }
}