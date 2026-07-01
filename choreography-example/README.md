# Choreography Example

A .NET Azure Functions example demonstrating the **choreography-based saga pattern** using Azure Service Bus topics and subscriptions.

## Architecture

The flow consists of four Azure Functions:

1. **`CreateOrder`** (HTTP GET) — Publishes an `OrderCreated` event to the `order-events` topic.
2. **`SendEmail`** — Subscribes to `OrderCreated` and simulates sending a confirmation email.
3. **`BookInventory`** — Subscribes to `OrderCreated`, books stock, then publishes `OrderInventoryBooked` back to `order-events`.
4. **`CreateDelivery`** — Subscribes to `OrderInventoryBooked` and creates a delivery, publishing to `delivery-events`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (Azure Storage emulator)
- [Docker](https://www.docker.com/) (for Service Bus Emulator + SQL Edge)

## Configuration
Create `local.settings.json` inside the `src/ChoreographyExample.Functions` with the following content.
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ChoreographyExampleKey2024!;UseDevelopmentEmulator=true;"
  }
}
```

## Running the project

### 1. Start Azurite

```bash
azurite --silent
```

### 2. Start the Service Bus Emulator

```bash
cd src\ChoreographyExample.Functions\ServiceBusEmulator
docker compose up -d
```

Wait for both SQL Edge and the emulator containers to be healthy.

### 3. Start the Functions host

```bash
cd src\ChoreographyExample.Functions
func start
```

### 4. Trigger an order

```bash
curl http://localhost:7071/api/CreateOrder
```

The request will flow through the saga: `CreateOrder → SendEmail + BookInventory → CreateDelivery`. Each step logs to the console.

