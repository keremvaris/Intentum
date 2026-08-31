# Intentum Background Service Template

A background service that consumes behavior streams and infers intent.

## How to run

```bash
dotnet new intentum-backgroundservice -n MyWorker
cd MyWorker
dotnet run
```

## Customization

- Replace `MemoryBehaviorStreamConsumer` with Kafka or Redis stream
- Add persistence to store intent history
- Configure retry policy in appsettings.json
