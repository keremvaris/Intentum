using Grpc.Net.Client;

Console.WriteLine("=== gRPC Client Demo ===\n");
Console.WriteLine("This demo requires a running Intentum gRPC server.");
Console.WriteLine("Start the server, then run this client.\n");
Console.WriteLine("To start the server, create a minimal ASP.NET gRPC project:");
Console.WriteLine("  - Reference Intentum.Grpc and Intentum.Core");
Console.WriteLine("  - Call services.AddGrpc() and app.MapGrpcService<IntentumGrpcService>()");
Console.WriteLine("  - Run on http://localhost:5000\n");

// Uncomment when server is running:
// using var channel = GrpcChannel.ForAddress("http://localhost:5000");
// var client = new IntentumService.IntentumServiceClient(channel);
//
// var inferResponse = await client.InferAsync(new InferRequest
// {
//     Events =
//     {
//         new BehaviorEvent { Actor = "user", Action = "login", OccurredAt = DateTime.UtcNow.ToString("O") },
//         new BehaviorEvent { Actor = "user", Action = "purchase", OccurredAt = DateTime.UtcNow.ToString("O") }
//     }
// });
// Console.WriteLine($"Inferred: {inferResponse.Name}, confidence: {inferResponse.Confidence.Score}");

Console.WriteLine("See src/Intentum.Grpc/ for the proto definition and service implementation.");
