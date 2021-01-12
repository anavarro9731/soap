# SOAP

Soap is a framework and a platform for developing API services on Azure.

It stand for Service Oriented Architecture Platform

It has the following goals:
- Remove and abstract the repetitive infrastructure code from building services
- Allow a collection of services to scale both in terms of architecture and infrastructure without code changes
- Make Devops tasks simple

A service in the SOAP context is a collection of code which has the following purposes: 

- defines a unique set of data structures 
- has the sole authority to operate on those structures 
- defines a unique set of messages
- has the sole authority to publish events in its message schema
- has the sole authority to receive commands in its message schema
- is built on top of the SOAP framework imported via Nuget

There is an important division within each service between the "Domain" and the "Infrastructure".

The Domain is everything (e.g. code, config, etc) which is specific to the business of that service 
(e.g. in a logistics service this might be things like routes or vehicles, etc) 
while Infrastructure is everything else (e.g. saving a file, accessing a database, sending a message).

As highlighted in the goals, we try to meet all the Infrastructure requirements with the framework code 
so developers can focus on the Domain.

Other considerations:

- The cost of architectural scalability often comes at the price of requiring lots of flexible joints 
which corresponds to a lot of small, particular, repetitive Domain parts. Sometimes these can create 
extra work during the definition stage, but once defined they should remain stable and flexible throughout
the life of the system. The architecture should recover any initial costs spent defining these parts 
by keeping the cost of change low as the systems' feature-set grows.  

##Post-Install Solution 

Note: All packages referred to in this guide are available on the Soap Nuget feed
https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json

Along with the [readme.md](readme.md), the [changeLog.md](changelog.md) contains the history of important changes 
and programming samples for features.

After having created a new service using the create-new-service script in the Soap repo you will be left with 
a boilerplate app that has several c# projects and a react application.

## Service Frontends

Each service comes with a front-end application, though not all services need to develop them. 

If a service provides significant front-end functionality we will call that a **UI Service** and it will 
have certain UI related features that other services may not.

Each service front-end can talk to only one service back-end, therefore if the UI for a given service needs 
data from another service it will have to get it through it's own services' API.

## Service Backends

Each service is hosted as an Azure Function app and while there are several Functions present on the public API by 
default they represent various infrastructure capabilities, not domain capabilities.

Services use an envelope wrapper pattern and it is possible, you will never need to 
add another endpoint to your service, only new messages.

The executable project is the Your.Api.Afs project, this is the Azure function project 
and the one that contains all the endpoints, you should not in theory have to modify this project.

The other projects are:

- YourApi.Messages, this project contains all the messages, both commands and events
- YourApi.Constants, this project contains any domain constants shared between messages, logic, and possibly aggregates
- YourApi.Models, this project contains the domain models which are persisted to the database. Other models for 
  processes and messages are nested within those classes/assemblies.
- YourApi.Logic, this contains all the business logic, message handlers, processes, operations and the like
- YourAPi.Tests, this project contains unit tests that exercise the message pipeline using various messages.

We will discuss each of these project types and their associated classes in detail.

### YourApi.Constants

Probably the simplest, this library contains only constants. Should any of these constants be used in message contracts
it is very important that the definition of a constant in that case noy only mean something fixed at compile time 
but something that does not change over time. Message contracts are immutable, and while serious efforts are made to
ensure that models are not shared between messages and contracts, constants can be, but extreme care must be taken,
to ensure such constants never change. Putting constants that have been used in a message in a separate folder would be advised.

Constants should be either enums or in most cases derive from Enumeration or TypeEnumeration<> and follow the specific pattern
for these special classes with are designed for serialisation and key-value transfer as well as static elegance.

This class allows:

* a developer to reference hardcoded enumerations like the native c# equivalent but also to store them in the database when they are user data
* keeps a standard approach for returning reference data to API clients, and provides a natural key that is human-readable in front-end logic
* has an active flag which is useful to provide a soft-delete approach common to user and hardcoded data
* has an implicit conversion to string and serialises nicely
* works together with the `EnumerationAndFlags` class to pass a list of options and the results in one object

An example of a simple use can be found in the [Release Versions](Soap.Api.Sample/Soap.Api.Sample.Constants/ReleaseVersions.cs) file.

Such classes are useful in that they can be treated like enums in domain code, but they serialise to a key/value pair that
can be stored in the database or used in the front-end without any translation and should be the preference at all times.

One notable exception is the States enum used to record the state of a Stateful Process. The Stateful Process helper
methods are designed to work with straight enums, and since this code is always self-contained within the process file itself,
enum is the right choice here.

### YourApi.Messages

#### Quick facts:

- Messages are always one of 2 types: Commands or Events. Each type has base classes used to create messages.
- Messages are always processed asynchronously, and queries are modeled as a Get*X*Command followed by a Got*X*Event
- Messages have enforced naming conventions and datatypes [see CachedSchema](Soap.MessagePipeline/CachedSchema.cs) 
  and [C107](Soap.Api.Sample/Soap.Api.Sample.Messages/Commands/C107v1_CreateOrUpdateTestDataTypes.cs)
- Messages represent a "unit of work" in the platform, meaning that all stateful operations that take place as 
  a result of handling the message are either
committed or rolled back together.
- Messages, if they fail, are retried a configurable number of times and then moved to an error queue.
- Message names must follow a strict convention and this will be enforced by the platform at runtime. The schema will
  fail to generate if the convention is not followed, an error will result and the health check will not pass.
- Each message instance has a Unique Id and no service can receive the same message twice, if it does it will be discarded.
- Messages are unique to a service. While shared, they are always owned by a single service.
- Using a message from another service requires importing that services' message library from your Enterprise Feed-(TODO)

The message pipeline follows a pipes and filters pattern and all messages can only be sent/published from either 
a Process or StatefulProcess step.

Messages **must not** reference any external domain-specific models. In other words they can only refer to classes nested 
within the message itself. This means that adapter code must be written to move data off the command onto an 
internal model. This can be a bit tedious if the model is large and complex but it is the only way that services 
can remain backwards-compatible over time as the domain changes and it is a key element in service independence and stability
and cost of change. The message assembly should also not be changed to reference any new packages or existing assemblies.
Because message assemblies are imported into other services this can cause a raft of versioning issues and is the reason
that the Soap.PfMessageBase assembly is very basic.

Messages are a contract and **must** not be changed after they are are released to production.
They have to be versioned instead. (i.e. the message must be copied and changed so
C100v1_PlaceSaleCommand becomes C100v2_PlaceSaleCommand. Once version the earlier versions should be 
marked with a [Obsolete] flag to notify developers.

#### Commands 

Commands represent the set of write operations you can perform on the data of a service.

They are point-to-point, meaning they are always sent from one place (the UI, a service) to the service that owns the command.

They are sent over the Azure Service Bus where routing is handled by the Azure broker, based on the message type name. 

In the UI this happens automatically using the [Autoform](soapjs/app/url-fragments/test-data/CreateTestData.jsx) 
control to submit a form.

In service code you would use the [Bus.Send](Soap.Api.Sample/Soap.Api.Sample.Logic/Processes/P206SendLargeMessage.cs) 
method of a Process to do this. Bus.Send has an overload which also allows you to schedule the message to be sent at a point in the future.

Commands can use only the datatypes you see in [C107](Soap.Api.Sample/Soap.Api.Sample.Messages/Commands/C107v1_CreateOrUpdateTestDataTypes.cs)
and also any custom types which use those types, but only in a 1-1 relationship with the message, not in lists.

#### Events

Events notify another party of changes that occured to the data owned by a service. 

They are published by one service and subscribed to by one or more other parties. These other parties can be UI clients 
(via websockets) or other services via the Azure service bus. 

Event can use and of the datatypes you see in [C107](Soap.Api.Sample/Soap.Api.Sample.Messages/Commands/C107v1_CreateOrUpdateTestDataTypes.cs)
and also any custom types which use those types, including in lists. 

##### Publishing

In service code you would use the [Bus.Publish](Soap.Api.Sample/Soap.Api.Sample.Logic/Processes/P205PingPong.cs) 
method of a Process or StatefulProcess to do this. By default, events will be published on the Bus to any subscribing services, 
and also to the websocket client that send the message whose handler is publishing when that is the source of the current 
"context message". This can however be changed by passing an `EventVisibility` constant as an overloaded parameter of the
Publish function. If for example, you know that the only interested party in an event would ever be the websocket sender
then you can specify this and save the needless publish on the bus or the alternative where the context message originated with
a websocket client, but the events the websocket client is not interested in the events its message has created. Finally, this
overload can be used in limited cases to send Broadcast messages, to either all Bus subscriptions or all Websocket clients
irrespective of how the context message was created. However doing so is an expensive operation and should be considered carefully.

##### Subscribing

Subscribing to an event, is as simple as creating a handler for it, and then [registering that handler](#message-registration)
You can subscribe to events you publish as well as those belonging to other services.

#### Other considerations 

- Each service has a messages assembly which is versioned and published separately from the service itself. 
  This makes it possible for two services with a circular dependency to avoid an endless cycle of recompilation in 
  an attempt to get the latest version of each other's messages.
- It is also important to note that a design decision to use Nullable types only on messages was made
and while there are certainly cons to this approach, it was taken mainly to avoid the complexities
of losing the intent of the caller when transforming from JSON and back. A special [Required] attribute
was created to allow for requiring fields in messages and this is checked both in the incoming and
outgoing stages of the message pipeline. However, there is no compile time checking for this, and
so it is very important that messages all have unit tests that exercise as much logic around creating
messages as possible. Decisions on how to handle the nullable nature of incoming message properties
should not sit in one general transformation or place, it is the responsibility of the specific receiver
(as there can be multiple) to decide how to handle the nullable message data. 
  A `(msg.property ?? defaultValue)` expression
should be the default way to handle the initial consumption of such values.

### YourApi.Models

The models assembly includes the models that are persisted to the database. By default these include the special `ServiceState`
model which is used to control service upgrades and store service meta data. Also the `User` class which while extendable should
not be modified as it's properties are required by the platform. 

Models can be broken down into one of 3 class types: Aggregates, Entities and Value Types.

Aggregates are complex models that reference Entities and/or Value Types but cannot be referenced by other aggregates.
They are the root objects which are persisted to the Database and the Data Access Layer will only work with this type.
They provide the Id property automatically along with several other properties used by the Data Access Framework
Aggregates **should never** reference other aggregates, they should only ever hold an Id reference.

Entities are simply classes which have an Id property and inherit from the `Entity` base class.
While Value Types are data structures which have no Id and derive their identity from the sum of their values, should
one of the values in a value types change it is not considered a new type. (e.g. an Address or a Color).

Entities and Value can and should be nested in the Aggregate that uses them if they are expected to only be used by a single aggregate.
When this is not the case they can be saved in their own file in a corresponding folder.

All models should remain purely data-centric and should have only 2 types of members:

- Static constructors
- Public serialisable properties

If you want to ask questions about an aggregates state you can create a corresponding class of *pure* 
extension methods in the same file for that purpose. This is also a design principle used heavily throughout the 
framework itself and a move towards a more functional programming style.
e.g.
```
class Car : Agggregate {
  public string Make { get; set; }
  public int? Year { get; set; } 
 }
static class CarFunctions {
  public static bool IsClassic(this Car c) => c.Year < DateTime.UtcNow.Year - 20;
}
```

### YourApi.Logic

This assembly contains 5 types of objects:
- *Handlers* which define what the processors for a message are
- A *HandlerRegistration* class which makes a message active on the endpoint
- *Operations* which manipulate data (i.e. Aggregates)
- *Processes* both stateful and transient which can have several functions
  - to send command and/or publish events
  - to determine, if necessary through logic, which is the appropriate operation(s) to call 
  - to obtain, if necessary through logic, the parameters for any operation calls
  - if stateful, to control the logical flow of a series of related messages received over time
- *Queries* which are used internally to abstract common data retrieval logic
 

We will cover each of these in details.

#### Handlers

These are small classes which have a fixed interface based on the interface [IMessageFunctionsClientSide<>](Soap.Interfaces/IMessageFunctions.cs)

which defines four basic things for a given message type:

- The `Handle` method which determines the single Process or Operation that will be the first step in the 
  imperative chain of steps to which the message or its data will be passed.
* The `Validate` method which determines how the message will be validated before it is accepted for 
  processing. Usually this call the msg.Validate() syntax validator on the message properties, but it
  doesn't have to and it can also perform any additional logic such as that which may require database
  access. It the only method in the handler which is allowed to have any logic of it's own.
- The `HandleFinalFailure` method defines what should happen when a message of this type which
  has failed repeatedly and reached it's retry limit. Usually this will result in some kind of notification
  being generated for someone to review. A database record is automatically created for each message
  in the form of the MessageLogEntry class and in most cases this could be queried by any monitoring 
  views without the need to create new models for this purpose.
- The `HandleWithTheseStatefulProcesses` method is a property getter which defines and array of StatefulProcess types
  that can respond to a message of this type **if** a StatefulProcessId header has been defined. This header must
  be set by the sender. If so, the pipeline will look to see if there are any StatefulProcesses' which have a 
  `IContinueThisProcess.Continue()` method defined for this message type, and if so will attempt to load 
  that StatefulProcess from storage and pass the message to it. One message can trigger several StatefulProcesses.
  
It is important to note that if a `Handle` method is defined for a message and there are also a list of
`HandleWithTheseStatefulProcesses` defined, only one of those two ways of handling the message will be executed
and the other will be ignored based purely on the presence of a StatefulProcessId header.

If a message has multiple versions each version will require it's own handler. In this case the handlers
for the earlier versions will need to transform the message first and avoid having multiple paths for processing
competing logic, except when this is specifically desired but most of the time when this is the case
a message with a different name and not version is a more suitable choice. 
This should be always done by with a local static 
[Upgrade function in the handler](Soap.Api.Sample/Soap.Api.Sample.Logic/Handlers/C111v1Functions.cs).

#### Handler Registration

Handlers are only operational once they are 'registered' by adding a line to 
the [HandlerRegistration](Soap.Api.Sample/Soap.Api.Sample.Logic/HandlerRegistration.cs) class.
Consideration has been given to whether these should be loaded by reflection and while this is not
a bad idea, the ability to disable (or not enable a new) handler has it's benefits particularly in
regard to allowing a new handler but not ready handler or a handler in which a bug has been found
to be present in the codebase but inactive.

Receiving a message which has no handler will throw an error.

#### Understanding The Logic Pipeline

When a Handler or any of the logic pieces (i.e. Processes or Operations) call into a next step, they use
a special set of methods that ensure they are calling on the pieces allowed from the current step.
This is done using the local this.Get<> function in the format of:

`this.Get<P200SomeProcess>().Call(s => s.BeginProcess)(msg or custom params);` to call a Process

`this.Get<SomeOperations>().Call(s => s.TheOperation)(msg or custom params);` to call an Operation

`this.Get<SomeQuery>().Call(s => s.TheQuery)(queryParams);` to call an internal Query

You should ***NEVER*** make a call from one logic step to another by any other means. If the thing you
want to do is not available via the this.Get<> method then the signature of the class you want to use
is probably wrong (e.g. wrong base class) or you are trying to do something the platform does not want you to.

Processes and Operations can and should take messages directly. However when they have more than one caller
you will need to create overloads on the Process or Operation to support the multiple entry points.

As the domain evolves taking a message directly may not be preferable in some cases.

e.g. Where there are multiple logic levels involved (ProcessA>ProcessD>Operation) and the messages'
data has already been transformed into something else. In these cases a similar logic will apply
where the lower level piece (Process2) may accept inputs for the same function in different ways

Scenario|ProcessD method
---|---
From Process1 using a custom model|ProcessD.DoX(CustomModel1)
From some message handler using the message as the model directly|ProcessD.DoX(Message1)
From another Process, ProcessB where ProcessB>ProcessD using a custom model|ProcessD.DoX(CustomModel2)

Adapters could be created, but it was decided that the responsibility for this transformation should
sit with the callee as it can often contain business logic which could be missed in some circumstances
e.g. Where the same input can come from multiple routes (e.g. Stateful Processes rather than handlers)
and the caller forgets to use the adaptor in one of them.

There is also the question of which class should own the "CustomModels" described in the example.
It may seem logical that the caller defines its model and the callee simply handles the different
models, but it has been found that in many cases multiple callers can suitably share a similar model
and so to reduce the overall model count, the models for logic piece method arguments should be defined
in the **logic piece that receives them** even when the model is designed for to serve a particular caller.

#### Operations

`Operations<TAggregate>` are classes which are designed to hold the logic for modifying an `Aggregate`.
Each operation class modifies only one `Aggregate` type. This is reinforced by the fact that the
class is generically typed. There will be only one `Operations<TAggregate>` class for each `Aggregate` type.
Because an `Aggregate` is a complex object, this means that any change to any of the classes
in the `Aggregate` structure can only be made by calling a method on the Operation. For example,
if we have an Order object with a collection of OrderLines, if you want to modify an OrderLine
you must call a method on the Order operation such as .AddOrderLine(). There will never be an
OrderLine Operation class.

Operations also behave as state-machines for the aggregate they represent and can be explicit about this
by including a property of type `EnumerationFlags` on the aggregate used to determine it's current state.
Though in simple cases, the Guard statements in the operation methods are enough to validate the implicit state.

#### Other important considerations for Operations:

* Operations should never call other operations, if you find you need to modify another `Aggregate`
  in the logic of an Operation you need to add a `Process` above which calls 2 separate Operations.

* Operation method arguments should be of the format (id, data) which forces you to find the
  aggregate in the parent Process rather than in the Operation which reinforces the right design

* Function in operations should always follow a strict pattern of defining up to 3 steps as local
  functions
  - Validation(of_incoming_data)
  - DetermineChange(data, out change)
  - Execute(change)

* If the public functions of Operations need to share common logic they can do so via private methods.

#### Processes
TODO

#### Queries

These are static helper functions which abstract common query logic. They can only be used 
via the this.Get<> method from Processes or Operations, not handlers.

There is nothing fancy about them. They should be pure functions, each of which has access to a `DataStoreReader` 
from the base class and returns whatever `Aggregate` or derivative model(s) are useful to the caller. 

Remember, the output of these function can never be attached directly to any Event or Command. They need
to be adapted to the nested models in that message first using a Model-Message adapter.

[Query Predicates](Soap.Api.Sample/Soap.Api.Sample.Logic/Queries/Predicates/TestData.cs) 
are reusable predicate delegates allows for re-use of common predicates among Queries. 

Other considerations:
- Queries have a special property DirectDataReader which is not available at present to 
Processes and Operations. This is a version of the DataStore read interface which goes
direct to the database and ignores are actions already taken in the previous session.
It makes possible extra features like Skip() and Take() with OrderBy() which simply cannot
  be reliably determined any other way.

#### Transactions

At present each message handler has a commit phase where the unit of work is executed
(all queued state changes are processed) and this takes place in a logical transaction
so all or none are committed. If the commit phase depending on the reason and current state
an attempt will be made either to complete or to rollback the current transaction.

There are some complexities to how this is implemented and in some rare cases the data could
be left in an inconsistent state, but this is not likely to happen frequently. There are good
comments and intentionally explicit code in the
[ContentWithMessageLogEntryExtensions](Soap.MessagePipeline/ContextWithMessageLogEntryExtensions.cs) class
where the majority of the Unit Of Work code sits which explain each step in detail.

#### Error Handling

Errors can be raised in the domain code of a service primarily by utilising `Guard.Against()`
which takes in a boolean expression or lambda returning a boolean as it's first arguments.

This would represent an unacceptable state of the code that should be treated as
an error e.g. `Guard.Against(user.HasPaid == false)`.

Errors raised during the domain code are caught by the message pipeline
that is processing the message and logged automatically to
as both a `MessageLogEntry` aggregate stored in the database as well as to Serilog.

Both the `MessageLogItem` and the Serilog message include relevant information
such as stack trace, custom error messages, error codes and profiling information.

Guard.Against has two overloads. One allows you to specify a different message to
external clients than internal ones (internal being your own logs, external being a UI).

The second takes only one error message and specifies a sensitivity which defaults
to ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients but could
also be MessageIsSafeForInternalClientsOnly in which case a generic error message
specified in the IApplicationConfig.DefaultExceptionMessage is used for external clients.

The logic conditions around how error messages are generated is complex and its best to
check out the [FormattedExceptionInfo](Soap.Context/Exceptions/FormattedExceptionInfo.cs) class
for details.

#### Specific Error Codes

In some cases specific error codes can be raised rather than just passing a string.
However, error codes should only be created and used when it is expected that the sender
of a message that fails can perform some sort of corrective action based on the code in the response,
either automatically or by prompting a user.

There is no value in creating an error code for each and every error,
as you can create a custom message when raising errors and relevant debug
information will be included in the error logs even without error codes.

It is important that each message have it's own set of error codes in order
to maintain independence of each message contract. And these should be
created

Specific errors with error codes can be raised through the use of the `code` parameter of `Guard.Against() `.

#### Message Validation Error Codes


### Shared Domain Logic Error Codes

In the situation where a specific error is raised in shared domain logic
(e.g. in a process called by two separate messages) the platform provides the ability to define an error code

mapper for a given message handler. An error code mapper will swap a shared domain logic error code for a message specific error code, if an error with the domain error code is raised during execution.

This is key to keeping each message contract free of ties to internal objects which can change independently of message contracts.

This is done by implementing the `MessageHandler.IMapErrorCodesFromDomainToMessageErrorCodes` interface in the message handler for the message that wants to use a domain error code.

IMapErrorCodesFromDomainToMessageErrorCodes is basically a list of pairs of Guids, one for the local code and one for the shared code.

If a shared domain logic error code is not mapped then it will not be returned as part of the response to a message.

Shared domain logic error codes are still defined by implementing `IHaveApiErrors<TErrorCodeEnum>` on the process or operation rather than the message.

In this case `TErrorCodeEnum` is a nested enum within that process or operation.
TODO cleanup


#### Authentication and Authorisation

This feature is still being developed and not available yet in the master branch TODO

#### Authentication 

Authentication is achieved technically through the use of attributes.

* No attribute on a message means that that it can be called without being logged in or without
  a service identity. 

* The AuthenticationRequiredAttribute requires that the identity token contain a valid identity
  recognisable by any of the authentication schemes supported on the recipient service.

#### Authorisation is achieved technically through the use of attributes.

Authentication is achieved technically through the use of attributes.

* The AuthorisationRequiredAttribute takes a Guid parameter and requires identity sending the message 
  to have that message permission in the token they attach to the message in order for it be processed by the recipient.
  
Message permissions are added as claims to the users token when they authenticate successfully.

Included in these claims are also some additional related objects, namely, Roles and Permission Groups.

### YourApi.Tests

This assembly is used to exercise the messages in your service.

#### What are they?

Domain tests are in-memory Xunit.net tests written in C# which execute a message handler which:

* check to see if the state changes that occur as a result of the message reflect the correct behaviour

* check to see that any data returned has the right qualities.

The unit chosen in these tests is quite large, it is the entire pipeline for that message.
That is a tactical choice, namely because each message represents a unit of work that must succeed or fail together,
and because more granular levels of testing can become too burdensome to maintain.

We want to write at least one domain test for each message, but we can and should often write more
than one to account for different outcomes in the code
dependent on the incoming message parameters, including, but certainly not limited to, error scenarios.

There are several types of state changes that can occur as the result of any message which we want to test:

* database changes
  * including changes to StatefulProcess states
* published events
* sent commands
* calls into any other I/O bound code: eg. random HTTP calls, 3rd party libraries
* notifications sent 

Where state changes as long as we capture them all we have captured the entire 
effect of the message upon the system we can test very effectively.

#### How do we write a test?

Test are written in a project which inherits from a Soap.Test package.
This gives each test access to a `Test` base class giving you various helper methods.
The tests follow a standard, Arrange/Given, Act/When, Assert/Then pattern.

In the Arrange section you will add Aggregates to the database and/or send Commands 
and Events into the pipeline which will execute logic and then in turn modify the database.
You do this with one of two helper methods `SetupTestBySendingAMessage` or `SetupTestByAddingAnAggregate`
Both approaches are useful in setting up a test scenario's environment and while 
this is too short a guide to detail the pros and cons of each a mixture is often best.

In the Act section you will send a Command, Event, or Query into the pipeline and this is the message being tested.
You do this using the `TestMessage` helper method.

Finally, in the Assert section you will check to see if the state changes and/or return values were correct.
You do this using the `Result` object which has various properties for validating the final state of the system.

Following the Xunit pattern of setup in the constructor and assertions in the methods of the class, 
you will create one test per scenario with multiple methods if there are multiple things to assert.

Each assertion is a logical one, not a physical one. This means that you may have multiple 
Assert... statements in one method so long as they are all serving one logical assertion 
(e.g. the Customer aggregate was updated)

#### How do we write assertions.

For the above scenarios we have different approaches to asserting the changes.

* For outgoing message we have an InMemoryMessageBus which collects the messages for assertion which can be
  queried using the `Result.Bus` class

* For database changes done via we have an InMemoryDatabase whose post-message state can be queried
  using teh `Result.Datastore` class

* For other I/O bound calls such as in third parties we use a technique called “wedge-testing”.
  Wedge-testing works a bit like a mock on a function call, but it does not require us to have an
  interface to mock on that we have to pass down through the layers of code, and is therefore ideal for working
  with 3rd party and legacy code where we want to disrupt the code as little as possible. It is similar to mock
  but works by collecting a special "event" when the IO/bound code is about to execute. This event is then passed to
  the function which calls the I/O bound code. In a test scenario, this event data is never passed and instead
  a result specific in the test setup is return instead. In production the event data is passed to the
  I/O bound call and execution continues normally. You can see an example of this being used internally
  to "gate" the calls to the blob storage API [here](Soap.Context/BlobStorage/BlobStorage.cs) and you can
  see the required setup in a test [here](Soap.Api.Sample/Soap.Api.Sample.Tests/Messages/Commands/C105/TestC105v1.cs)
  and [here](Soap.Api.Sample/Soap.Api.Sample.Tests/Messages/Commands/C106/TestC106v1.cs)

* For notifications there is a notifications property `Result.Notifications`

There is also a boolean `Success` property which says whether the test succeeded and an
`UnhandledError` which can be used in conjunction with each other to test specific
[error scenarios](Soap.Api.Sample/Soap.Api.Sample.Tests/Messages/Commands/C104/TestC104MessageDiesWhileSavingUnitOfWork.cs)






### Processes

Processes are classes which tie multiple aggregate operations together. They encapsulate some form of business process. 
All the steps in the process usually share one primary responsibility:

Obtain the Id of an Aggregate which needs modification and pass it to an Operation class to make the modification.

As the application grows the "process layer" will get deeper. At first processes are added which call operations, then later processes which call other processes. As this happens some careful refactoring will be needed. Considerations include:

* How validation is split between processes and operations

* Whether processes receive commands as input or their own DTOs. If the process is used by more than one other process then it cannot take a command, in this case the process will need to expose it's own nested model, or use an aggregate , and translate between command models in a similar way to how messages have private models.

#### Other important considerations:

* Processes should always have at least 2 steps, and call more than one operation

* Except for the dependencies received in the constructor, process objects should not have any class level variables

* Processes cannot modify aggregates but they are allowed to send messages, save files, or any other IO-bound operations that the platform may allow which are not an Aggregate data structure. In these cases each of these non-Aggregate operations should be a step in the operation.

### StatefulProcesses

StatefulProcesses technically share the definitive aspect of a vanilla `Process` which is *they perform multiple operations*. The difference is that they do this over time, utilizing two or more messages to do so.

StatefulProcesses will have very little business logic of their own, choosing to defer most of it to vanilla Processes or to Operations both of which can be called directly from a `StatefulProcess`.

StatefulProcesses also have a few special properties.

One is called `References`. This is effectively a Dictionary which allows you to save data which needs to be retained between steps, or to tie steps together. It is called `References` because it is most commonly used to store Guids obtained during the processing of one message in a later message. Date/Times, Counts, other simple types can also be stored in the References collection, but anything more complicated than that should be stored in a new `Aggregate` which is modified by the `StatefulProcess`.

Another is called `ProcessId`. This is just the Id of the instance of the `StatefulProcess` which is created by the framework at runtime. Simple, but very important if you are going to send a message to another framework and you need a reply. In order for the other service to know how to connect it's message back to your `StatefulProcess` in order to continue it, you will need to provide it with this Id when you send it a message.

Finally, StatefulProcesses have a series of methods designed to help you manage it's *state*. These are:

* `GetState()`

* `HasState()`

* `AddState()`

Each of these methods take an enum (which you must define inside the `StatefulProcess`).

The types of logic a `StatefulProcess` is allowed to have are:

* Logic that validates the current state of the StatefulProcess or a proposed transition.

* Logic that determines how to transation to the next state

* Any logic required to call a certain process or operation from incoming message

#### Using Blob Storage

#### Using Emails and Notification

Emails sent through `EmailSender` are gated through the use of the `MessageAggregator` which allows for them to be wedge tested as part of domain tests:

```c#

this._endpoint.MessageAggregator.When<EmailSender.SendingEmail>().Return(Task.CompletedTask);

```

## Instrumentation

### Health Check

Each service has a built-in health check available at the following function address http://{environment host}/api/CheckHealth

### Logging

All messages ingoing and outgoing are logged via the Serilog library in two primary ways:

1. All incoming messages get a MessageLogEntry aggregate created which has extensive information including
all failed and successful attempts at processing the message. 

2. All messages also get a Serilog entry persisted to Azure Application Insights Traces collection which
   contains all inward and outward data with the exclusion of blob data on large message and fields explicitly 
   marked as sensitive (e.g. password) with the Destructurama [NotLogged] attribute which are stripped. 
   In addition all logged, messages include Profiling Data, this is a breakdown of the time spend executing the 
   message showing all the IO-bound operations (e.g. database calls) and CPU-operations. 
  
3. A Logger class is also on the Process and Operation base classes to log custom messages. 
   The messages you see logged to the console when running locally are the same messages logged to 
   Azure App Insights in cloud environments.
   
## Devops

### Service Config

Each service built on the platform requires configuration. 
Configuration is basically all the data the service needs in order set itself up to start processing messages.
The configuration is a range of variables varying from database connection strings to api urls.

Most of this is taken care of for you automatically by the devops scripts, but it is important to understand some key points
- Each service when it starts queries for a file in the service.config repo that corresponds to the environment it is running in.
- By default all the variables required in this file are set by the scripts
- If you need to add custom config properties and they are common to all environments just add them as constants in the logic
assembly
- If you have custom config that differs per environment (e.g. a third party api with its own environments and separate urls)
then you will need to add a property to the Config classes via a new interface or base class in your service.config repo and
  then in the Logic code you need to cast the generic ApplicationConfig to your new custom type. You can do this using 
  the following code in any Process or Operation `GetConfig<MyConfigType>()`

### Pushing Changes

You can push your code on the master branch or any feature branch at any time to the repo.
However if you want your code to publish changes to any of the built-in environments you need to use the provided scripts.

Some important points:
- There are 2 directly publishable environments
  - Vnext (where the code for the next version sits)
  - Release (where the code for either the last release branch created sits)
    - This environment could be behind or ahead of Vnext depending on where you are in the release cycle
    - This environment is also used for hotfix changes
- Feature branches (those created from master) cannot be published to an environment 

Once you have created your own service using the create-new-service.ps1 script as described in the [readme](readme.md)
you will have an important devops script you need to use to publish changes to your service
This is called pwsh-bootstrap.ps1 and you need to execute this script each time you start a new terminal session in
which you intend to publish a change.

This script will load all the custom powershell modules from the web into memory for that session.
Once this is complete you can run the following 2 scripts:
1. Run -PrepareNewVersion
2. Run -CreateRelease

Run -PrepareNewVersion is the script you use when you want to publish a change from:
1. Master to Vnext to update vnext
2. A release branch to Release either to test a hotfix or to test a new release
If you are wanting to make a change to release, you should have the release branch checked out in GIT.
   And if you want to update vnext you should have the master branch checked out.
   The script will take the current branch into consideration when deciding how to make the update.
When you run the script it will ask you some semantic questions to figure out how to change the version number
   properly and then finally ask you if you want to push the changes.
   If you answer yes, the environment will be updated immediately after the push, otherwise it will
   occur when you push the commit as the changes to version file required to cause the update have
   at this point already been committed.
   
Run -CreateRelease is the script you use when you want to create a new release branch.
You run this script from the master branch and it has the following effects
1. It creates a new release branch with the correct version based on the current version of the master branch.
   (This could be a Major or Minor release)
 2. It asks if you want this new branch to be pushed right away triggering the update of the environment
In most cases you would say yes. One important note is that if this is the first ever release branch pushing
    it will trigger the creation of all the release environment infrastructure which can take some time.
    3. It properly increments the master branch moving it ahead of the new release branch 
  4. It returns you to the master branch

#### Hotfixes

When you create a release branch the last digit of the version 1.1.0 will always be 0.
If you checkout this branch make changes and then run Run -PrepareNewVersion the version
will be changed to 1.1.1 where the last 1 is considered as the first hotfix to 1.1.1 and will be pushed to release

There is only one release environment and in the event that you are testing a hotfix and a new
release at the same time it will be a last-in-wins situation as to whose version is published at 
any given moment.

#### Publishing to Production

TODO

In many cases these scripts will give you errors if you are trying to do something you should not
and you just need to follow the instructions given.