#Usage

The OData Migration extension gives OData v4 ASP.NET Core 2.2+ services the ability to accept OData v3 requests.  This extension is limited
to the OData JSON format.  There are two steps to configure your service to use OData Migration.  

In this tutorial, both of these simple steps will be shown, and a more detailed explanation will follow.

## Step 1: Configuring Services
```
public static void ConfigureServices(IServiceCollection services)
{
	// your code here
	services.AddODataMigration();
	// your code here
}
```

AddODataMigration takes no arguments, and adds the following to your service collection:

1) OData Migration input formatter, 
2) OData Migration output formatter,
3) OData Migration filters.

The OData Migration input and output formatters observe the incoming request to see if it contains either of the specific OData version headers.
These headers are "dataserviceversion: 3.0" and "maxdataserviceversion: 3.0"  If either of these headers are present in the request, the input formatter
will deserialize the request as a OData v3 request, and the output formatter will return JSON that is OData v3 compliant.  Both of these formatters
make use of the OData Edmx contract to validate requests and responses just as they would be validated in v4.

The OData Migration filters are used to 1) add version and content ID headers to batch requests, and 2) catch exceptions in batch requests
and send them back as internal server errors with content ID headers attached.

## Step 2: Configuring IApplicationBuilder
```
public static void Configure(IApplicationBuilder builder)
{
  ...
  IEdmModel model = ...
  string v3Edmx = ...
  builder.UseODataMigration(v3Edmx, model);
}
```

Calling UseODataMigration inserts the middleware responsible for translating incoming request URLs.  For example, an OData version 3 request URL might look like:

```
https://localhost/v3/Product(guid'02951787-4c1a-4dff-a917-a04b21b40ad3')
```

whereas the equivalent OData version 4 request URL looks like:

```
https://localhost/v3/Product(02951787-4c1a-4dff-a917-a04b21b40ad3)
```

OData Migration extension's middleware will take care of this conversion for you automatically, provided that you are using JSON and have either the
dataserviceversion or maxdataservice header in your request headers.

As parameters to the method, you must supply the EDMX string of the v3 model of your service.  This string is used for model validation during translation and is also returned
when an OData version 3 request asks for the metadata of your service.  You must also supply the V4 IEdmModel that will be used for model validation during translation.
