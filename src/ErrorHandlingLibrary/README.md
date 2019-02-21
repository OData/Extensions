# Introduction 
This project aims to solve a problem with inconsistent error messages propagated from workloads to Microsoft Graph. Some examples include variants of "BadRequest", such as "badrequest", "bad request", "unknownError". By leveraging this library, workloads can use ODataError objects with a prescribed hierarchy of error messages, ensuring that app developers of Microsoft Graph can experience a smoother experience (and subsequently their app users). Additionally, this project benefits Microsoft Graph itself by addressing support costs and cleaning up the telemetry; the consistency of the error codes will "pre-triage" issues, saving a large amount of time.

# Getting Started
1. The project uses VS2017 and the WebAPI .NET Core sample uses .NET Core 2.0.
2. The `samples` folder contains samples for OData Core, WebAPI classic, and WebAPI .NET Core.

# Build and Test
1. Clone the project to your local environment.
2. Launch the solution file with VS2017 and simply build everything.
3. `Microsoft.Workload.Errors.UnitTests` contains the unit tests. Run them with Visual Studio.