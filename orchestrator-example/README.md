# Orchestration Example

A .NET Azure Functions example demonstrating the **orchestration pattern** using Azure Service Bus topics and subscriptions with a standalone orchestrator process.

In this pattern, a central **orchestrator** controls the flow by sending commands to services and waiting for their responses. Unlike choreography, services do not autonomously decide what to do next — they simply execute commands and report back.

## Architecture

Two independent processes communicate via Service Bus:

### Process 1: `OrderProcess.Functions` (Azure Functions host)

| Function | Trigger | Role |
|---|---|---|
| **`CreateOrder`** | HTTP GET `/api/CreateOrder` | Publishes `OrderPlaced` to `orchestrator-events` and returns immediately |
| **`BookInventory`** | `orchestrator-commands` / `book-inventory-sub` | Books stock, publishes `BookInventoryResult` |
| **`SendEmail`** | `orchestrator-commands` / `send-email-sub` | Sends confirmation email, publishes `SendEmailResult` |
| **`CreateDelivery`** | `orchestrator-commands` / `create-delivery-sub` | Creates a delivery, publishes `CreateDeliveryResult` |

### Process 2: `OrderOrchestrator` (Standalone console app)

| Service | Role |
|---|---|
| **`OrderOrchestratorService`** | Subscribes to `orchestrator-events`, coordinates the 3 steps sequentially |

### Flow Diagram

```
┌─ Client ──────────────────────────────────────┐
│  HTTP GET /api/CreateOrder                     │
│  └→ CreateOrder stores order, publishes        │
│     OrderPlaced to orchestrator-events          │
│     then returns HTTP 200 immediately           │
└────────────────────────────────────────────────┘
                        │
                        ▼  (async via Service Bus)
           ┌──────────────────────────┐
           │ OrderOrchestratorService │  (separate process)
           │                          │
           │  1. BookInventoryCommand │→ orchestrator-commands
           │     ← BookInventoryResult│← orchestrator-events
           │     (if failed → stop)   │
           │                          │
           │  2. SendEmailCommand     │→ orchestrator-commands
           │     ← SendEmailResult    │← orchestrator-events
           │                          │
           │  3. CreateDeliveryCommand│→ orchestrator-commands
           │     ← CreateDeliveryResult│← orchestrator-events
           │                          │
           │  All done.               │
           └──────────────────────────┘
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (Azure Storage emulator)
- [Docker](https://www.docker.com/) (for Service Bus Emulator + SQL Edge)

## Configuration

Create `local.settings.json` inside `src/OrderProcess.Functions`:

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

Set the same `ServiceBusConnection` environment variable before running the orchestrator (or pass it as a system environment variable).

## Running the project

### 1. Start Azurite

```bash
azurite --silent
```

### 2. Start the Service Bus Emulator

```bash
cd src\OrderProcess.Functions\ServiceBusEmulator
docker compose up -d
```

Wait for both SQL Edge and the emulator containers to be healthy.

### 3. Start the Functions host

```bash
cd src\OrderProcess.Functions
func start
```

### 4. Start the Orchestrator (in a separate terminal)

```bash
cd src\OrderOrchestrator
dotnet run
```

### 5. Trigger an order

```bash
curl http://localhost:7071/api/CreateOrder
```

The request flows: `CreateOrder → (orchestrator) → BookInventory → SendEmail → CreateDelivery`. The HTTP response returns immediately; the orchestrator processes the steps asynchronously. Each step logs to its respective console.

## Project Structure

```
src/
├── OrderProcess.Functions/     Azure Functions (HTTP + Service Bus triggered)
│   ├── CreateOrder.cs                 HTTP entry point
│   ├── BookInventory.cs               Books inventory on command
│   ├── SendEmail.cs                   Sends email on command
│   ├── CreateDelivery.cs              Creates delivery on command
│   ├── Models/                        Event DTOs
│   ├── Services/                      In-memory stores
│   └── Infrastructure/                Service Bus setup
│
└── OrderOrchestrator/                 Standalone orchestrator process
    ├── Program.cs                     Entry point
    ├── OrderOrchestratorService.cs    BackgroundService (saga coordinator)
    ├── PendingOrderStore.cs           TCS-based request correlation
    └── Messages.cs                    Command/event DTOs
```
