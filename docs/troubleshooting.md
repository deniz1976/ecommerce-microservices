# Troubleshooting

## Docker Desktop Is Not Running

Symptom:

```text
dockerDesktopLinuxEngine pipe not found
```

Meaning:

Docker Desktop is closed or Docker engine is not ready.

Fix:

1. Open Docker Desktop.
2. Wait until Docker says engine is running.
3. Run the command again.

Check:

```powershell
docker version
```

## Missing Runtime Environment Variables

Symptom:

```text
Missing required environment variables
```

Fix:

Run:

```powershell
.\scripts\check-runtime-env.ps1
```

Then set the missing values in PowerShell:

```powershell
$env:ConnectionStrings__CatalogDb = "postgresql://..."
$env:RabbitMq__ConnectionString = "<managed-rabbitmq-uri>"
```

## Migrations Fail

Command:

```powershell
.\scripts\run-migrations.ps1
```

Common causes:

- wrong database name
- wrong password
- missing SSL parameters
- database not created in Neon
- network issue
- expired/rotated credentials

Checks:

```text
Does the database exist?
Does the connection string point to the correct database?
Does the connection string include sslmode=require?
```

## RabbitMQ Connection Fails

Common causes:

- `RabbitMq__ConnectionString` missing
- wrong username
- wrong password
- wrong virtual host
- URI uses `amqp://` instead of `amqps://`
- CloudAMQP instance is paused or unavailable

Check:

```powershell
.\scripts\check-runtime-env.ps1
```

Then check CloudAMQP dashboard.

## Redis Connection Fails

Current local Redis comes from Docker Compose:

```text
redis:7-alpine
```

Basket in Docker connects to:

```text
redis:6379
```

If Redis fails:

```powershell
docker compose ps
docker compose logs redis
```

If running Basket outside Docker without Redis, it can fall back to in-memory storage.

## Smoke Test Fails

Command:

```powershell
.\scripts\smoke-test.ps1
```

If health checks fail:

```powershell
docker compose ps
docker compose logs api-gateway
docker compose logs ordering-api
```

If workflow probe fails:

Check:

- gateway running on port 5080
- Identity API healthy
- Inventory API healthy
- Ordering API healthy
- RabbitMQ reachable
- database connection strings set

Health-only smoke test:

```powershell
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
```

The script prints the component name and exact health URL before every request. If readiness times out, use the final named URL to distinguish the gateway's own health endpoint from an Ocelot downstream health route.

## Gateway Route Fails

Check local gateway:

```powershell
Invoke-WebRequest http://localhost:5080/health/live
```

Check service through gateway:

```powershell
Invoke-WebRequest http://localhost:5080/gateway/health/ordering
```

If direct service works but gateway route fails:

- inspect `ocelot.json`
- inspect `ocelot.Docker.json`
- check environment name

## Tests Fail

Run:

```powershell
dotnet test ECommerce.sln
```

If package restore fails:

```powershell
dotnet restore ECommerce.sln
```

If central package management complains:

Check:

```text
Directory.Packages.props
```

Package versions should be there, not inside individual test csproj files.

## Secret Scan Fails

`validate-local.ps1` scans for secret-like values.

If it fails:

1. Remove real secret from tracked file.
2. Put placeholder in docs or config.
3. Use environment variable instead.

Bad:

```text
RabbitMq__ConnectionString=<real-secret-written-to-a-file>
```

Good:

```text
RabbitMq__ConnectionString=
```

or process env:

```powershell
$env:RabbitMq__ConnectionString = "<managed-rabbitmq-uri>"
```

## Port Already In Use

Ports used:

```text
5080 gateway
5283 catalog
5041 basket
5265 ordering
5054 inventory
5004 payment
5187 shipping
5234 notification
5090 identity
6379 redis
```

Find process:

```powershell
netstat -ano | findstr :5080
```

Stop old Docker containers:

```powershell
docker compose down
```

## Outbox Messages Not Publishing

Check:

- RabbitMQ connection
- service logs
- outbox tables
- consumer service running

Relevant tables:

```text
OutboxMessage
OutboxState
```

If messages stay there, broker delivery may be failing.

## Consumer Not Receiving Messages

Check:

- consumer service is running
- RabbitMQ queue exists
- consumer count in CloudAMQP
- messages stuck in ready/unacked state
- service logs

Relevant table:

```text
InboxState
```

## Notification SignalR Does Not Receive Messages

Check:

- Notification API running
- client connected to `/hubs/notifications`
- client joined correct group
- events are being consumed
- notification records are created

Client group format:

```text
customer:{customerId}
```
