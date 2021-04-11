# Change Log

- v.6.4.0 
  - initial
- v.6.6.0
  - claim schema updates to support AzureAD
  

# BackLog

- MUST
    - Save Roles into UserProfile so you can check them serverside
    - Crud Fragment Generator
    - Custom Domain and CDN (before live)
    - Stripe Integration (before payments)
- SHOULD
    - Create script to set LIVE config variables and swap Release with LIVE (e.g. Run -InstallProd (from release branch))
    - Extend Missing Property Proxy to cover properties on child objects of a message
    - Consider refresh tokens and how to implement if not already active
    - Create a flag to publish only soap packages (by leaving azurefunctionproject param blank)
- COULD
    - Cache some further relatively static I/O things obtained with IO bound ops in pipeline
    - Adding a Special Flag or Tag to Denote builds that were sent to production (which will need new Run -InstallProd switch which runs az slot swap and tags so when your looking at the release branch you can see which version went to production)
    - Inter-service queries from a UI service via REST with a new Function (ServiceQuery) which doesn't load pipeline just does a DB read, in non-durable, non-recoverable fashion but is quick.
    - CheckHealth to Test SignalR somehow
    - Fixing DateTime fragility by abstracting all date/time functionality and/or using NodaTime rather than DateTime. (NodaTime will not pass IsSystemType checks may need to adjust those functions (there are private versions))
    - Client-Side Batching using native Azure ServiceBus feature
    - Add DataStore "Document Size Limit" on write error. It will still fail with a less friendly native error from CosmosDB as-is. Also, hitting the 2MB limit should not happen really without bad design.
    - Further cleanup of i18next module in soapjs
    - Add AssemblyReferences files for all assemblies
