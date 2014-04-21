# The Splunk Software Development Kit for C# 
### Version 2.0 pre-release

**Note: The Splunk SDK for C# is in-development and is not currently supported.**

The Splunk Software Development Kit (SDK) for C# contains library code and 
examples designed to enable developers to build applications using Splunk.

Splunk is a search engine and analytic environment that uses a distributed
map-reduce architecture to efficiently index, search, and process large 
time-varying data sets.

The Splunk product is popular with system administrators for aggregation and
monitoring of IT machine data, security, compliance and a wide variety of 
other scenarios that share a requirement to efficiently index, search, analyze,
and generate real-time notifications from large volumes of time series data.

The Splunk developer platform enables developers to take advantage of the 
same technology used by the Splunk product to build exciting new applications
that are enabled by Splunk's unique capabilities.

## Supported platforms

.NET 4.5, Windows Phone 8.1, Xamarin IOS/Android.

## What's new in Version 2.0.

Version 2.0 introduces new modern APIs which leverage the latest .NET platform advancements. 

* Async - All APIs are 100% async supporting the new [async/await](http://msdn.microsoft.com/en-us/library/hh191443.aspx) features.
* All APIs follow .NET Guidelines and abide by FX Cop and Style Cop rules.
* Reactive Extensions - Splunk query results implement [IObservable<T>](http://msdn.microsoft.com/en-us/library/dd990377(v=vs.110).aspx) allowing usage with the [.NET Reactive Extensions](http://msdn.microsoft.com/en-us/data/gg577610).
* Support for multiple platforms - The Splunk API Client (Splunk.Client.dll) in the new version is a [Portable Class Library](http://msdn.microsoft.com/en-us/library/vstudio/gg597391(v=vs.110).aspx).

Below is an example of a simple Normal Search:
```csharp
using Splunk.Client;

var service = new Service(Scheme.Https, "localhost", 8089));

//connect
await service.LoginAsync("admin", "changeme");

//create a job
var job = await service.StartJobAsync("search index=_internal | head 10");

//get the results
var searchResults = await job.GetSearchResultsAsync());

//loop through the results
foreach (var record in searchResults)
{
    Console.WriteLine(string.Format("{0:D8}: {1}", ++recordNumber, record));
}
```

## Compatability

Version 2.0 is a rewrite of the existing SDK, and introduces completely new APIS. Existing applications will not recompile using the new version.

Version 2.0 includes a subset of capability in the existing SDK focusing on the most common scenarios that we have seen customers using. The major focus areas are Search / Jobs, Configuration and Modular Inputs.

Below is a breakdown of the areas covered:

* Login
* Access Control - users and passwords
* Searches - Normal, Blocking, One Shot and Export
* Jobs
* Saved Searches
* Configuration / Config Properties
* Indexes
* Inputs (Receiving Simple and Streamed events)
* Applications
* Modular Inputs

For detailed API coverage, see this [excel sheet](http://test.com)

We will publish guidance on how to migrate existing CSharp SDK applications to use the new SDK.

## Getting started with the Splunk SDK for C# 

The Splunk SDK for C# contains library code and examples that show how to 
programmatically interact with Splunk for a variety of scenarios including 
searching, saved searches, data inputs, and many more, along with building 
complete applications. 

The information in this Readme provides steps to get going quickly. In the 
future we plan to roll out more in-depth documentation.

### Requirements

Here's what you need to get going with the Splunk SDK for C#.

#### Splunk

If you haven't already installed Splunk, download it at 
<http://www.splunk.com/download>. For more information about installing and 
running Splunk and system requirements, see the
[Splunk Installation Manual](http://docs.splunk.com/Documentation/Splunk/latest/Installation). 

#### Splunk SDK for C# 

[Get the Splunk SDK for C#](https://github.com/splunk/splunk-sdk-csharp-pcl/archive/master.zip). Download 
the ZIP file and extract its contents.

If you are interested in contributing to the Splunk SDK for C#, you can 
[get it from GitHub](https://github.com/splunk/splunk-sdk-csharp) and clone the 
resources to your computer.

#### Visual Studio

The Splunk SDK for C# supports development in Microsoft Visual Studio 2012 and later. The 
minimum supported version of the .NET Framework is version 4.5. It also supports Windows Phone 8.1 and Xamarin. Visual Studio 
downloads are available on the 
[Visual Studio Downloads webpage](http://www.microsoft.com/visualstudio/downloads).

### Building the SDK

Before starting to develop custom software, you must first build the SDK. Once 
you've downloaded and extracted—or cloned—the SDK, do the following:

1. At the root level of the **splunk-sdk-csharp-pcl** directory, open the 
**splunk-sdk-csharp-pcl.sln** file in Visual Studio.
2. On the **BUILD** menu, click **Build Solution**.

This will build the SDK, the examples, and the unit tests.

### Examples and unit tests

The Splunk SDK for C# includes full unit tests. Examples are a work in progress.
iI
### Changelog

The **CHANGELOG.md** file in the root of the repository contains a description
of changes for each version of the SDK. You can also find it online at
[https://github.com/splunk/splunk-sdk-csharp/blob/master/CHANGELOG.md](https://github.com/splunk/splunk-sdk-csharp/blob/master/CHANGELOG.md). 

### Branches

The **master** branch always represents a stable and released version of the SDK.
You can read more about our branching model on our Wiki at 
[https://github.com/splunk/splunk-sdk-csharp/wiki/Branching-Model](https://github.com/splunk/splunk-sdk-java/wiki/Branching-Model).

## Documentation and resources

If you need to know more:

* For all things developer with Splunk, your main resource is the [Splunk
  Developer Portal](http://dev.splunk.com).

* For more about the Splunk REST API, see the [REST API 
  Reference](http://docs.splunk.com/Documentation/Splunk/latest/RESTAPI).

* For more about about Splunk in general, see [Splunk>Docs](http://docs.splunk.com/Documentation/Splunk).

## Community

Stay connected with other developers building on Splunk.

<table>

<tr>
<td><em>Email</em></td>
<td><a href="mailto:devinfo@splunk.com">devinfo@splunk.com</a></td>
</tr>

<tr>
<td><em>Issues</em>
<td><a href="https://github.com/splunk/splunk-sdk-csharp-pcl/issues/">
https://github.com/splunk/splunk-sdk-csharp/issues</a></td>
</tr>

<tr>
<td><em>Answers</em>
<td><a href="http://splunk-base.splunk.com/tags/csharp/">
http://splunk-base.splunk.com/tags/csharp/</a></td>
</tr>

<tr>
<td><em>Blog</em>
<td><a href="http://blogs.splunk.com/dev/">http://blogs.splunk.com/dev/</a></td>
</tr>

<tr>
<td><em>Twitter</em>
<td><a href="http://twitter.com/splunkdev">@splunkdev</a></td>
</tr>

</table>

### Contributions

If you want to make a code contribution, go to the 
[Open Source](http://dev.splunk.com/view/opensource/SP-CAAAEDM)
page for more information.

### Support

This product is currently in development and officially unsupported. We will be triaging any issues filed by the community however and addressing them as appropriate. Please [file] (https://github.com/splunk/splunk-sdk-csharp-pcl) issues for anything problems that you encounter.

### Contact Us

You can reach the Dev Platform team at devinfo@splunk.com.

## License

The Splunk SDK for C# is licensed under the Apache License 2.0. Details can be 
found in the LICENSE file.
