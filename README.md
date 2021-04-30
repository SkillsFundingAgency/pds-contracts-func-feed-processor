# Manage your education and skills funding - Contracts atom feed processor

## Introduction

Contracts feed processor is a serverless azure function that reads contract event atom feed. This function is a timer triggered function.

### Getting Started

This product is a Visual Studio 2019 solution containing several projects (Azure function application and service layers, with associated unit test and integration test projects).
To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally.

### Installing

Clone the project and open the solution in Visual Studio 2019.

#### List of dependencies

|Item |Purpose|
|-------|-------|
|Azure Storage Emulator| The Microsoft Azure Storage Emulator is a tool that emulates the Azure Blob, Queue, and Table services for local development purposes. This is required for webjob storage used by azure functions.|
|Azure function development tools | To run and test azure functions locally. |
|Azure service bus | When the feeds are processed, a message will be created for the contract processor to continue processing of contract events. Service bus cannot be set up locally, you will need an azure subscription to set-up. |
|Contracts Atom Feed | Atom feed with contract events for processing. |
|Audit API | Audit API provides a single shared service to audit events in "Manage your education and skills funding". |

#### Azure Storage Emulator

The Storage Emulator is available as part of the Microsoft Azure SDK. Azure functions require it for local development.

#### Azure function development tools

You can use your favourite code editor and development tools to create and test functions on your local computer.
We used visual studio and Azure core tools CLI for development and testing. You can find more information for your favourite code editor at <https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local>.

* Using Visual Studio - To develop functions using visual studio, include the Azure development workload in your Visual Studio installation. More detailed information can be found at <https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs>.
* Azure Functions Core Tools - These tools provide CLI with core runtime and templates for creating functions, which can be used to develop and run functions without visual studio. This can be installed using package managers like `npm` or `chocolately` more detailed information can be found at <https://www.npmjs.com/package/azure-functions-core-tools>.

#### Azure service bus

Microsoft Azure Service Bus is a fully managed enterprise message broker.
Publish-subscribe topics are used by this application to decouple approval processing.
There are no emulators available for azure service bus, hence you will need an azure subscription and set-up a service bus namesapce with a topic created to run this application.
Once you have set-up an azure service bus namespace, you will need to create a shared access policy to set in local configuration settings.

#### Contracts Atom feed

Atom feed with contract events for processing, the atom entry's content will have contract payload that will be processed by feed processor. A sample entry content can be found [here](Pds.Contracts.FeedProcessor\Pds.Contracts.FeedProcessor.Services.Tests\Documents\11_03\ESIF-9999-v5-Partial.xml).

#### Audit API

Audit API can be found at <https://github.com/SkillsFundingAgency/pds-shared-audit-api>.

### Local Config Files

Once you have cloned the public repo you need the following configuration files listed below.

| Location | config file |
|-------|-------|
| Pds.Contracts.FeedProcessor.Func | local.settings.json |

The following is a sample configuration file

```json
{
  "IsEncrypted": false,
  "version": "2.0",
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "WEBSITE_TIME_ZONE": "GMT Standard Time",
    "ServiceBusConnection": "replace_ServiceBusConnectionString",
    "ContractEventsSessionQueue": "replace_QueueName",
    "TimerInterval": "* */5 * * * *"
  },
  "AzureStorageAccountOptions": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "BlobAccessOptions": {
      "XmlStorageContainer": "xmlfiles",
      "RetryCount": 3,
      "Delay": "0.00:00:30"
    },
    "TableAccessOptions": {
      "ConfigTableName": "TableName",
      "PartitionKey": "PartitionKey",
      "DeltaBackOff": "00:00:15",
      "MaxAttempts": 3
    }
  },
  "AuditApiConfiguration": {
    "ApiBaseAddress": "replace_local_audit_api_or_stub",
    "ShouldSkipAuthentication": "true",
    "CreateAuditEntryEndpoint": {
      "Endpoint": "/api/audit"
    }
  },
  "FeedReaderOptions": {
    "ApiBaseAddress": "replace_local_contract_notifications_api_or_stub",
    "FcsAtomFeedSelfPageEndpoint": "/api/contracts/notifications",
    "Authority": "https://login.microsoftonline.com/",
    "ShouldSkipAuthentication" : "true",
    "HttpPolicyOptions": {
      "HttpRetryCount": 3,
      "HttpRetryBackoffPower": 2,
      "CircuitBreakerToleranceCount": 5,
      "CircuitBreakerDurationOfBreak": "0.00:00:15"
    }
  }
}
```

The following configurations need to be replaced with your values.
|Key|Token|Example|
|-|-|-|
|FeedReaderOptions.ApiBaseAddress|replace_local_contract_notifications_api_or_stub|<http://localhost:5001>|
|AuditApiConfiguration.ApiBaseAddress|replace_local_audit_api_or_stub|<http://localhost:5002/>|
|MonolithServiceBusConfiguration.ConnectionString|replace_ServiceBusConnectionString|A valid azure service bus connection string|
|ServiceBusConnectionString|replace_ServiceBusConnectionString|A valid azure service bus connection string|
|ContractEventsSessionQueue|replace_QueueName|atom-feed-queue|

## Build and Test

This API is built using

* Microsoft Visual Studio 2019
* .Net Core 3.1

To build and test locally, you can either use visual studio 2019 or VSCode or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Contribute

To contribute,

* If you are part of the team then create a branch for changes and then submit your changes for review by creating a pull request.
* If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.
