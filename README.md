# DynamicAPI
It's a lot of writing work to create client - server application in .NET using a rest api. You need to create the services (ideally using interfaces), controller, clients - apart from the actual client application.

Especially if you want to modify an existing monolith to a distributed application it may lead to a monster task - that was a task I had to do ...
Of course, there are some alternatives instead of rest but all of them were not usable for me - maybe they didn't fit my requirements or the switch (e. g. to RPC) would require extreme effort.

So I looked for ways to keep the effort as low as possible and I found Refit.

[Refit](https://github.com/reactiveui/refit) is a great library for creating type-safe rest clients just out of an interface class. This would significantly reduce the effort. But the part of writing the controller classes based on the services would remain.

## Create an interface
DynamicAPI can be used for classes and interfaces. I recommend using an interface and put it in an independent class library. This can be used directly as a client via refit.

```C#
public interface IPeopleService
{
    [Get("/people")]
    Task<IEnumerable<Person>> GetPeople();

    [Get("/people/search")]
    Task<IEnumerable<Person>> SearchPeople([Query] string? FirstName, [Query] string? LastName, [Query] string? Company);

    [Get("/people/company/{company}")]
    Task<IEnumerable<Person>> GetPeople([AliasAs("company")] string company);

    [Get("/people/person/{id}")]
    Task<Person> GetPerson([AliasAs("id")] string id);

    [Post("/people/person")]
    Task CreatePerson([Body] Person person);

    [Delete("/people/person/{id}")]
    Task DeletePerson([AliasAs("id")] string id);

    [Put("/people/person")]
    Task UpdatePerson([Body] Person person);
} 
```

Take a look at the [Refit repo](https://github.com/reactiveui/refit) to find a description how to decorate methods and parameters. Just add a reference to the DynamicAPI. The DynamicAPI library already contains the Refit package.

## Create a server application
Create a new ASP.NET Core-Web-API. Add a reference to the DynamicAPI.

1. Create a new service which implements the interface. This service handles the logic behind the endpoints (see the mock service in the samples).

```C#
public class PeopleService : IPeopleService
{

    public PeopleService()
    {
        
    }


    public async Task CreatePerson([Body] Person person)
    {
        ...
    }

    public async Task DeletePerson([AliasAs("id")] string id)
    {
        ...    
    }

    public async Task DeletePersonNew([AliasAs("id")] string id)
    {
        ...
    }

    public async Task<IEnumerable<Person>> GetPeople()
    {
        ...
    }

    public async Task<IEnumerable<Person>> GetPeople([AliasAs("company")] string company)
    {
        ...
    }

    public async Task<IEnumerable<Person>> GetPeopleNew()
    {
        ...
    }

    public async Task<Person> GetPerson([AliasAs("id")] string id)
    {
        ...
    }

    public async Task<IEnumerable<Person>> SearchPeople([Query] string? FirstName, [Query] string? LastName, [Query] string? Company)
    {
        ...
    }

    public async Task UpdatePerson([Body] Person person)
    {
        ...
    }
}
```

2. Inject the service in the dependency injection in Program.cs

```C#
builder.Services.AddScoped<IPeopleService, PeopleService>();
```

3. Add the service as DynamicsAPI controller in Program.cs

```C#
app.AddDynamicController<IPeopleService>();
```

That's all folks - your server API should be working now.

## Error handling
In case of an error, the controller throws a internal server error (500). So, if we throw an exception in your data service (e.g. if the person was not found), of course, we want to return the correct status code to the client.
If you throw a DynamicsAPIException, you can provide a status code, which should be returned to the client.

First add the following line to Program.cs

```C#
app.UseDynamicAPIExceptionHandler();
```

This indicates the server to use a exception handling for the DynamicsAPIException and to return the status code you set.

```C#
public async Task<Person> GetPerson([AliasAs("id")] string id)
{
    var person = _people.FirstOrDefault(x => x.Id == id); //Get the person from database

    if(person == null)
    {
        throw new DynamicAPIException(System.Net.HttpStatusCode.NotFound, "Person not found!");
    }

    return person;
}
```

## Client side
Creating a client using refit is pretty easy. All you need to do is to inject the client into the dependency injection in Program.cs - for further details take a look at [Refit repo](https://github.com/reactiveui/refit).

```C#
builder.Services.AddRefitClient<IPeopleService>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7030"));
```

## Authorization
Of course, some of the apis we build should be secure and requires authorization. DyanmicsAPI is designed to use policies to handle authentication and authorization. The policies are created as known. Take a look at [Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-8.0) if you need further information.

To get DynamicAPI to use authorization you just neet to add the **RequireAuthorization** attribute to the interface or the interface method.

```C#
[RequireAuthorization("user")]
public interface IPeopleService
{
    ...

    [RequireAuthorization("admin")]
    [Delete("/people/person/{id}")]
    Task DeletePerson([AliasAs("id")] string id);
}
```

The attribute on top of the interface ensures that the entire controller is just accessible if the calling client / user hits the policy named 'user'. In this example we additionally added the attribute to the method delete. In this case the method is only accessible if the client / user hits all of the policies - 'user' **and** 'admin'.
You can add multiple policies to the **RequireAuthorization** attribute. The caller would need to hit **all** of them.

## Additional parameters for handling http requests
There are some attributes for configuring the server how to handle the requests (time out, logging ...). These attributes can be set for the entire interface or for individual methods. If the attribute is defined at both interface level and method level the method settings will override the interface settings.

**IgnoreInDynamicAPIAttribute**
Indicates DynamicAPI to skip this method for server controller.

**InformationAttribute**
*Some parameters of this attribute affects the openAPI settings*
GroupName: Name for grouping the endpoint. Look at [GitHub](https://github.com/dotnet/aspnetcore/issues/36414) for further information 
Description: Description of the end point 
Summary: Defines the summary for the end point
Order: Sort order

Look at [Microsoft Learn - OpenApiRouteHandlerBuilderExtensions Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.openapiroutehandlerbuilderextensions?view=aspnetcore-8.0) for more information

**HttpLoggingAttribute**
LoggingFields: 
RequestBodyLimit: Set the request body limit for the endpoint. If ist set to -1 results in using the default value defined for global HttpLogging.
ResponseBodyLimit: Set the request body limit for the endpoint. If ist set to -1 results in using the default value defined for global HttpLogging.

Look at [Microsoft Learn - HttpLoggingEndpointConventionBuilderExtensions Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.httploggingendpointconventionbuilderextensions?view=aspnetcore-8.0) for more information

**RequestTimeoutAttribute**
Setting the time out policy for the end point(s).

Look at [Microsoft Learn - RequestTimeoutsIEndpointConventionBuilderExtensions Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.requesttimeoutsiendpointconventionbuilderextensions?view=aspnetcore-8.0) for more information

**TagsAttribute**
*This attribute affects the openAPI settings*
Add tags to the controller and / or the method.
*If tags are set for both interface level and method level, the tags will be merged.*

Look at [Microsoft Learn - OpenApiRouteHandlerBuilderExtensions Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.openapiroutehandlerbuilderextensions?view=aspnetcore-8.0) for more information

**MetaDataAttribute**
Set meta data for controller or method endpoint.

Look at [Microsoft Learn - RoutingEndpointConventionBuilderExtensions Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.routingendpointconventionbuilderextensions?view=aspnetcore-8.0) for more information
