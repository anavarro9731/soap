# Change Log

v10.0.0-alpha
Breaking Changes:
 - Tests which use blob storage, should now use the new framework functions rather than setupMocks
This means using SetupTestByAddingABlob() and Result.BlobStorage
   
Key Features:
- Datastore.WithoutEventReplay now supports projections
By calling Read() or ReadActive() with the new map parameter you can setup a mapping which will be
  used server-side to return only a subset of the fields. The DTO you are mapping to must implement the
  Aggregate base class. When writing the map function you do not need to map the base class, 
  the framework will set them for you.

Other Changes:
- The 2MB size limit on UOW has been removed, any size is possible though perhaps not performant.
- Incoming messages via DirectHttp can exceed previous 2MB limit

# BackLog

- L1
    - Custom Domain and CDN and GoLive Scripts - Create script to set LIVE config variables and swap Release with LIVE (e.g. Run -InstallProd (from release branch))
      Adding a Special Flag or Tag to Denote builds that were sent to production (which will need new Run -InstallProd switch which runs az slot swap and tags so when your looking at the release branch you can see which version went to production)
    - recovery mode handler
    - call satellite service directly receive hub response
    - replace AggregateList control with new AccordionList
- L2
    - signalr inbound
    - Extend Missing Property Proxy to cover properties on child objects of a message
    - Review use of Refresh tokens
    - Crud Fragment Generator
- L3
    - Cache some further relatively static I/O things obtained with IO bound ops in pipeline
    - Inter-service queries from a UI service via REST with a new Function (ServiceQuery) which doesn't load pipeline 
      just does a DB read, in non-durable, non-recoverable fashion but is quick. The problem is writing testing for this you need
      a new SetupTestByRegisteringServiceQuery response and you wouldn't be able to use dependent method in dev without the other service running.
    - CheckHealth to Test SignalR
    - Fixing DateTime fragility by abstracting all date/time functionality and/or using NodaTime rather than DateTime. (NodaTime will not pass IsSystemType checks may need to adjust those functions (there are private versions))
    - Consider client-Side Batching using native Azure ServiceBus feature (right now batches are done by an array of commands on a command)
    - Add DataStore "Document Size Limit" on write error. It will still fail with a less friendly native error from CosmosDB as-is. 
    - Further cleanup of i18next module in soapjs
    - Add AssemblyReferences files for all assemblies
