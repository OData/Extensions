﻿# OData Extensions Libraries
 Build  | Status
--------|---------
Client Rolling | <img src="https://identitydivision.visualstudio.com/OData/_apis/build/status/Extensions/OData.Extensions-master%20-%20Rolling"/>
Client Nightly | <img src="https://identitydivision.visualstudio.com/OData/_apis/build/status/Extensions/OData.Extensions-master%20-%20Nightly"/>

## 1. Introduction

The [OData Extensions Libraries](https://github.com/OData/Extensions) (or OData ExtensionsLib, for short) project includes the implementation of Extensions for OData libraries and framework. It is a fully open sourced project maintained by Microsoft OData team. The libraries are recommended to be adopted to build new OData Services with asp.net core 2.1

[OData](http://www.odata.org/ "OData") stands for the Open Data Protocol. It was initiated by Microsoft and is now an [ISO](https://www.oasis-open.org/news/pr/iso-iec-jtc-1-approves-oasis-odata-standard-for-open-data-exchange) approved and [OASIS](https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=odata) standard. OData enables the creation and consumption of REST APIs, which allow resources, identified using URLs and defined in a data model, to be published and edited by Web clients using simple HTTP requests.

For more information about OData, please refer to the following resources:

- [OData.org](http://www.odata.org/)
- [OASIS Open Data Protocol (OData) Technical Committee](https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=odata)

**For how to adopt this and related libraries to build or consume OData service, please refer to the following resources:**

- [Build an OData v4 Service with RESTier Library](http://odata.github.io/RESTier/#01-01-Introduction)
- [Build an OData v4 Service with OData WebApi Library](http://odata.github.io/WebApi/#01-02-getting-started)
- [OData .Net Client](http://odata.github.io/odata.net/#04-01-basic-crud-operations)

## 2. Project structure

The project currently has 1 branch: [master](https://github.com/OData/Extensions/tree/master).

**master branch:**

This master branch is the development branch for extensions for ODataV4 7.x and is now most actively iterated. It builds upon the ODataLib 7.x release which is now on [ODataLib release branch](https://github.com/OData/odata.net/tree/release) and produces only [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) libraries. The branch builds with Visual Studio 2017 only.

For each profile above, it has the following libraries:

- [OData Client Factory](http://www.nuget.org/packages/Microsoft.OData.Client/) (namespace `Microsoft.OData.Extensions.Client`): The client extensions library is built on top of ODataLib that has adopted builder and factory pattern to create the OData client to be used for issuing OData queries and consuming OData JSON payloads. See also [HttpClientFactory](https://github.com/aspnet/HttpClientFactory) for http client counterpart.

For these libraries, we accept bug reports and pull requests. The corresponding fixes and implementations will be included into every new release.

## 3. Building, Testing, Debugging and Release

### 3.1 Building and Testing in Visual Studio

Simply open the shortcut `Extensions.sln` at the root level folder to launch a solution that contains the product source and relevant unit tests. Should you see the need to modify or add additional tests, please see the `sln` folder for the whole set of solution files.

Here is the usage of each solution file (the `Extensions.sln` shortcut opens the one marked default):

- ODataClientFactory.sln (default) - Product source built with .Net Standard 2.0, and contains corresponding unit tests. _Recommended_ for doing general bug fixes and feature development.

Each solution contains some test projects. Please open it, build it and run all the tests in the test explorer. For running tests within ODataClientFactory.sln, you need to open Visual Studio IDE as **_Administrator_** so that the test services can be started properly.

### 3.2 One-click build and test script in command line

Open Command Line Window with "**Run as administrator**", `cd` to the root folder and run following command:

`build.cmd`

This will build the full product and run all tests. It will take about 60 minutes. Use the to ensure your change compiles and passes tests before submitting a pull request.

Optionally, you can run following command:

`build.cmd quick`

This will build a single set of product Dlls and run unit tests. It will take about 5 minutes. Use this for quickly testing a change.

Here are some other usages or `build.cmd`:

- `build.cmd` or `build.cmd Nightly` - Build and run all nightly test suites.
- `build.cmd Quick` or `build.cmd -q` - Build and run all unit test suites (with less legacy tests thus faster).
- `build.cmd EnableSkipStrongName` - Configure strong name skip of OData libraries on your machine and build (no test run).
- `build.cmd DisableSkipStrongName` - Disable strong name skip of OData libraries on your machine and build (no test run).

Notes: If there is build error with message "build.ps1 cannot be loaded", right click "build.ps1" -> Properties -> "Unlock".

### 3.3 Debug

Please refer to the [How to debug](http://odata.github.io/WebApi/10-01-debug-webapi-source).

### 3.4 Nightly Builds

The nightly build process will upload a NuGet packages for ODataLib (Core, Edm, Spatial, Client) to the [MyGet.org odlnightly feed](https://www.myget.org/gallery/odlnightly).

To connect to odlnightly feed, use this feed URL: [odlnightly MyGet feed URL](https://www.myget.org/F/odlnightly).

You can query the latest nightly NuGet packages using this query: [MAGIC OData query](https://www.myget.org/F/odlnightly/Packages?$select=Id,Version&$orderby=Version%20desc&$top=4&$format=application/json)

### 3.5 Official Release

The release of the component binaries is carried out regularly through [Nuget](http://www.nuget.org/).

## 4. Documentation

Please visit the [ODataLib pages](http://odata.github.io/odata.net). It has detailed descriptions on each feature provided by OData lib, how to use the OData .Net Client to consume OData service etc.

## 5. Community

### 5.1 Contribution

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

There are many ways for you to contribute to OData .NET. The easiest way is to participate in discussion of features and issues. You can also contribute by sending pull requests of features or bug fixes to us. Contribution to the documentations is also highly welcomed. Please refer to the [CONTRIBUTING.md](https://github.com/OData/odata.net/blob/master/.github/CONTRIBUTING.md) for more details.

### 5.2 Support

- Issues: Report issues on [Github issues](https://github.com/OData/Extensions/issues).
- Questions: Ask questions on [Stack Overflow](http://stackoverflow.com/questions/ask?tags=odata).
- Feedback: Please send mails to [odatafeedback@microsoft.com](mailto:odatafeedback@microsoft.com).
- Team blog: Please visit [http://blogs.msdn.com/b/odatateam/](http://blogs.msdn.com/b/odatateam/) and [http://www.odata.org/blog/](http://www.odata.org/blog/).

### Thank you

We’re using NDepend to analyze and increase code quality.

[![NDepend](images/ndependlogo.png)](http://www.ndepend.com)

### Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
