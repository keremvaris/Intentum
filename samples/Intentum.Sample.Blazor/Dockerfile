# Build stage. Build context must be repo root (Render: Root Directory = empty).
# Sample references ../../src, so full repo tree is required.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "samples/Intentum.Sample.Blazor/Intentum.Sample.Blazor.csproj"
RUN dotnet publish "samples/Intentum.Sample.Blazor/Intentum.Sample.Blazor.csproj" -c Release -o /app/publish --no-restore

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS run
WORKDIR /app
COPY --from=build /app/publish .

# Render sets PORT at runtime; app listens on 0.0.0.0:PORT via Program.cs
EXPOSE 8080
ENTRYPOINT ["dotnet", "Intentum.Sample.Blazor.dll"]
