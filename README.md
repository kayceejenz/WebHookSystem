# WebHook Delivery Service

A reliable webhook delivery system that allows you to register HTTP endpoints, subscribe them to event types, and automatically delivers events with built-in retry logic, HMAC signature verification, and dead letter management.

## Overview

When something happens in your system (an order is placed, a payment fails, a user signs up), you publish an event. The WebHook Delivery Service matches that event to all interested subscribers and delivers it to their HTTP endpoints — retrying on failure and never silently losing a message.

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL and Redis)

### Running the Infrastructure

Start PostgreSQL and Redis using Docker Compose:

```bash
docker-compose up -d
```

This spins up:

| Service  | Image              | Port |
|----------|--------------------|------|
| Postgres | postgres:16-alpine | 5432 |
| Redis    | redis:7-alpine     | 6379 |

### Running the Services

You'll need to start three processes (each in a separate terminal):

```bash
# API (Swagger UI at http://localhost:5000)
dotnet run --project src/WebHookDeliveryService.Api

# Background Worker (processes deliveries and retries)
dotnet run --project src/WebHookDeliveryService.Worker

# Dashboard (Blazor UI at http://localhost:5001)
dotnet run --project src/WebHookDeliveryService.Dashboard
```

### Running Tests

```bash
dotnet test
```

## How It Works

### 1. Register a Webhook Subscription

Tell the system where to send events and which events you care about.

```bash
curl -X POST http://localhost:5000/api/webhooks \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://your-server.com/webhook-handler",
    "events": ["order.created", "payment.processed"],
    "maxRetries": 5,
    "baseDelaySeconds": 5,
    "maxDelaySeconds": 3600
  }'
```

You'll receive a **secret** in the response. Store it securely — you'll need it to verify incoming webhooks.

### 2. Ingest an Event

When something happens, publish an event to the system.

```bash
curl -X POST http://localhost:5000/api/events \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "order.created",
    "payload": "{\"orderId\": \"12345\", \"amount\": 99.99}",
    "idempotencyKey": "unique-request-id-abc"
  }'
```

The service responds with `202 Accepted`. It immediately finds every active subscription matching `order.created` and queues a delivery for each one.

### 3. Delivery

Background workers pick up queued deliveries and POST to each subscriber's URL with the following headers:

| Header                   | Value                                      |
|--------------------------|--------------------------------------------|
| `X-Webhook-Signature`    | `t={timestamp},v1={hmac_signature}`        |
| `X-Webhook-Timestamp`    | Unix timestamp of the delivery attempt     |
| `X-Webhook-Event-Id`     | The event's unique ID                      |
| `X-Webhook-Event-Type`   | The event type (e.g. `order.created`)      |
| `X-Webhook-Delivery-Id`  | The delivery record's unique ID            |

### 4. Verifying Signatures

Each delivery is signed with HMAC-SHA256 using your subscription's secret. Verify it on your end:

1. Extract the timestamp and signature from `X-Webhook-Signature` (format: `t={timestamp},v1={signature}`).
2. Compute `HMAC-SHA256(secret, "{timestamp}.{request_body}")`.
3. Compare the result to the received signature using a timing-safe comparison.
4. Reject the request if the timestamp is older than 5 minutes (replay protection).

### 5. Retry on Failure

If the subscriber's endpoint returns an error, the service retries automatically using exponential backoff:

```
delay = min(baseDelay × 2^(attempt - 1), maxDelay)
```

For example, with the defaults (`baseDelay = 5s`, `maxDelay = 3600s`, `maxRetries = 5`):

| Attempt | Delay   |
|---------|---------|
| 1       | 5s      |
| 2       | 10s     |
| 3       | 20s     |
| 4       | 40s     |
| 5       | 80s     |

Only specific HTTP status codes trigger retries: `408`, `429`, `500`, `502`, `503`, `504`. Responses with `2xx` status codes (`200`–`204`) are marked as successful.

### 6. Dead Letters

After all retries are exhausted, the delivery is moved to a **dead letter queue** with a 30-day retention period. From there you can:

- **Replay** — reset the delivery back to pending and try again from scratch.
- **Dismiss** — discard the dead letter.

### 7. Idempotency

Duplicate events are automatically deduplicated. Include an `idempotencyKey` in your event ingestion request, and the system will reject duplicates within a 24-hour window.

## API Reference

### Subscriptions

| Method   | Endpoint                                  | Description                      |
|----------|-------------------------------------------|----------------------------------|
| `POST`   | `/api/webhooks`                           | Create a subscription            |
| `GET`    | `/api/webhooks`                           | List all subscriptions           |
| `GET`    | `/api/webhooks/{id}`                      | Get a subscription               |
| `PUT`    | `/api/webhooks/{id}`                      | Update a subscription            |
| `DELETE` | `/api/webhooks/{id}`                      | Delete a subscription            |
| `POST`   | `/api/webhooks/{id}/regenerate-secret`    | Generate a new HMAC secret       |

### Events

| Method | Endpoint           | Description                    |
|--------|--------------------|--------------------------------|
| `POST` | `/api/events`      | Ingest an event                |
| `GET`  | `/api/events`      | List events (paginated)        |
| `GET`  | `/api/events/{id}` | Get an event                   |

### Deliveries

| Method   | Endpoint                      | Description                           |
|----------|-------------------------------|---------------------------------------|
| `GET`    | `/api/deliveries`             | List deliveries (filterable)          |
| `GET`    | `/api/deliveries/{id}`        | Get a delivery with attempts          |
| `POST`   | `/api/deliveries/{id}/retry`  | Manually retry a delivery             |

### Dead Letters

| Method   | Endpoint                        | Description                      |
|----------|---------------------------------|----------------------------------|
| `GET`    | `/api/dead-letters`             | List dead letters                |
| `GET`    | `/api/dead-letters/{id}`        | Get a dead letter                |
| `POST`   | `/api/dead-letters/{id}/replay` | Replay a dead letter             |
| `DELETE` | `/api/dead-letters/{id}`        | Dismiss a dead letter            |

### Dashboard

| Method | Endpoint              | Description                   |
|--------|-----------------------|-------------------------------|
| `GET`  | `/api/dashboard/stats`| Aggregated delivery statistics |

## Built-In Event Types

The system ships with these predefined event types, though custom types are also supported:

- `order.created` / `order.updated` / `order.deleted`
- `user.created` / `user.updated`
- `payment.processed` / `payment.failed`
- `custom`

## Dashboard

A Blazor Server dashboard is available at `http://localhost:5001` providing:

- Overview of delivery success/failure rates
- Subscription management
- Delivery and event logs with filtering
- Dead letter management with replay and dismiss actions

## Configuration

Connection strings are defined in `appsettings.json` for the API and Worker projects:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=webhook_delivery;Username=whd_user;Password=whd_pass",
    "Redis": "localhost:6379"
  }
}
```

## Testing

```bash
dotnet test
```

Unit tests cover HMAC signature generation/verification and exponential backoff calculation.
