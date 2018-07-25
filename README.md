# Azure Cosmos DB Scaler
Auto scaler for Azure Cosmos DB

## Master
![](https://noops-jp.visualstudio.com/0b69ecaf-dc1d-47b6-9696-8528d6e12537/_apis/build/status/1)

# Introduction
The purpose of this library is to flexibly scale Azure Cosmos DB. Throughput is changed by using information from Request Charges from individual client queries. Also, it is possible to extend scaling with custom logic. This library includes a minimal database client that is also extensible. For example, sending Cosmos DB telemetry to Application Insights.

## Requirements
- ASP.NET Core
- Azure Cosmos DB

## Usage
A NuGet package will be released in the near future. Currently this can be used with the below instructions.
- Clone this repository with `git clone https://github.com/NoOps-jp/azure-cosmosdb-scaler.git`
- Add to your own solution the [src/NoOpsJp.CosmosDbScaler solution](https://github.com/NoOps-jp/azure-cosmosdb-scaler/blob/master/src/NoOpsJp.CosmosDbScaler/NoOpsJp.CosmosDbScaler.sln).
- Add to your own project [src/NoOpsJp.CosmosDbScaler/NoOpsJp.CosmosDbScaler/NoOpsJp.CosmosDbScaler.csproj](https://github.com/NoOps-jp/azure-cosmosdb-scaler/blob/master/src/NoOpsJp.CosmosDbScaler/NoOpsJp.CosmosDbScaler/NoOpsJp.CosmosDbScaler.csproj) as a project reference.
- Register necessary parts in `Startup.Configure`. Refer to the sample in [samples/aspnetcore-web](https://github.com/NoOps-jp/azure-cosmosdb-scaler/blob/master/samples/aspnetcore-web)
