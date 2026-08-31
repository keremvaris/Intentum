# Intentum CQRS Template

A CQRS Web API with MediatR, FluentValidation, and intent inference.

## How to run

```bash
dotnet new intentum-cqrs -n MyCqrsApi
cd MyCqrsApi
dotnet run
```

## Architecture

- Commands/Queries via MediatR
- FluentValidation for request validation
- Intent inference on each request
- Serilog structured logging

## Adding a feature

1. Create a command: `Features/MyFeature/MyCommand.cs`
2. Create a handler: `Features/MyFeature/MyCommandHandler.cs`
3. Create a validator: `Features/MyFeature/MyCommandValidator.cs`
4. Register in DI
