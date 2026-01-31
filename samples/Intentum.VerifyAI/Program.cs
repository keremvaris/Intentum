using System.Net;
using System.Text;
using DotNetEnv;
using Intentum.AI.Embeddings;
using Intentum.AI.Models;
using Intentum.AI.OpenAI;
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Gemini;
using Intentum.AI.Mistral;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

// ---------------------------------------------------------------------------
// Intentum AI doğrulama: .env'de key set edilen (ve isteğe bağlı filtrelenen) sağlayıcılar için embedding + tam pipeline doğrular.
// Çalıştırma: dotnet run --project samples/Intentum.VerifyAI
// Sadece belirli sağlayıcılar: VERIFY_AI_PROVIDERS=Mistral veya VERIFY_AI_PROVIDERS=Mistral,OpenAI
// İstek/yanıt gövdesi: VERIFY_AI_VERBOSE=1 (isteğe bağlı VERIFY_AI_PROVIDERS ile birlikte kullanılabilir)
// .env.example'da tüm sağlayıcıların env değişkenleri listelenir.
// ---------------------------------------------------------------------------

Env.TraversePath().Load();

var verbose = string.Equals(Environment.GetEnvironmentVariable("VERIFY_AI_VERBOSE"), "1", StringComparison.OrdinalIgnoreCase);
var providersFilter = ParseProvidersFilter(Environment.GetEnvironmentVariable("VERIFY_AI_PROVIDERS"));

Console.WriteLine("Intentum × AI sağlayıcı doğrulama");
if (providersFilter.Count > 0)
    Console.WriteLine($"  (VERIFY_AI_PROVIDERS={string.Join(",", providersFilter)} → sadece bu sağlayıcılar çalıştırılıyor)");
if (verbose) Console.WriteLine("  (VERIFY_AI_VERBOSE=1 → istek/yanıt gövdesi gösteriliyor)");
Console.WriteLine();

var anyOk = false;

static HashSet<string> ParseProvidersFilter(string? env)
{
    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (string.IsNullOrWhiteSpace(env)) return set;
    foreach (var s in env.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        if (!string.IsNullOrEmpty(s)) set.Add(s.Trim());
    return set;
}

static bool ShouldRunProvider(string name, HashSet<string> filter)
{
    if (filter.Count == 0) return true;
    return filter.Contains(name);
}

// --- OpenAI ---
if (ShouldRunProvider("OpenAI", providersFilter) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
{
    if (verbose) Console.WriteLine(">>> OpenAI");
    try
    {
        var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL") ?? "https://api.openai.com/v1/";
        var options = new OpenAIOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
            EmbeddingModel = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL") ?? "text-embedding-3-small",
            BaseUrl = baseUrl.TrimEnd('/') + "/"
        };
        using var http = CreateHttpClient(verbose, options.BaseUrl!, "OpenAI", ("Authorization", "Bearer " + options.ApiKey));
        var provider = new OpenAIEmbeddingProvider(options, http);
        var result = provider.Embed("user:login");
        ValidateAndPrint("OpenAI", result);
        RunFullPipelineAndPrint(provider, "OpenAI");
        anyOk = true;
    }
    catch (OpenAIRateLimitException ex)
    {
        PrintOpenAIRateLimit(ex.RetryAfterSeconds, ex.ResponseBody);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        PrintOpenAIRateLimit(null, null);
    }
    catch (Exception ex) { Console.WriteLine($"  [FAIL] OpenAI: {ex.Message}"); }
}

// --- Azure OpenAI ---
if (ShouldRunProvider("Azure", providersFilter) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")) &&
    !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")))
{
    if (verbose) Console.WriteLine(">>> Azure OpenAI");
    try
    {
        var options = new AzureOpenAIOptions
        {
            Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!.TrimEnd('/') + "/",
            ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!,
            EmbeddingDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT") ?? "embedding",
            ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2023-05-15"
        };
        using var http = CreateHttpClient(verbose, options.Endpoint, "Azure OpenAI", ("api-key", options.ApiKey));
        var provider = new AzureOpenAIEmbeddingProvider(options, http);
        var result = provider.Embed("user:login");
        ValidateAndPrint("Azure OpenAI", result);
        RunFullPipelineAndPrint(provider, "Azure OpenAI");
        anyOk = true;
    }
    catch (AzureOpenAIRateLimitException ex)
    {
        PrintRateLimit("Azure OpenAI", ex.RetryAfterSeconds, ex.ResponseBody);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        PrintRateLimit("Azure OpenAI", null, null);
    }
    catch (Exception ex) { Console.WriteLine($"  [FAIL] Azure OpenAI: {ex.Message}"); }
}

// --- Gemini ---
if (ShouldRunProvider("Gemini", providersFilter) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GEMINI_API_KEY")))
{
    if (verbose) Console.WriteLine(">>> Gemini");
    try
    {
        var baseUrl = Environment.GetEnvironmentVariable("GEMINI_BASE_URL") ?? "https://generativelanguage.googleapis.com/v1beta/";
        var options = new GeminiOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")!,
            EmbeddingModel = Environment.GetEnvironmentVariable("GEMINI_EMBEDDING_MODEL") ?? "text-embedding-004",
            BaseUrl = baseUrl.TrimEnd('/') + "/"
        };
        using var http = CreateHttpClient(verbose, options.BaseUrl!, "Gemini", ("X-Goog-Api-Key", options.ApiKey));
        var provider = new GeminiEmbeddingProvider(options, http);
        var result = provider.Embed("user:login");
        ValidateAndPrint("Gemini", result);
        RunFullPipelineAndPrint(provider, "Gemini");
        anyOk = true;
    }
    catch (GeminiRateLimitException ex)
    {
        PrintRateLimit("Gemini", ex.RetryAfterSeconds, ex.ResponseBody);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        PrintRateLimit("Gemini", null, null);
    }
    catch (Exception ex) { Console.WriteLine($"  [FAIL] Gemini: {ex.Message}"); }
}

// --- Mistral ---
if (ShouldRunProvider("Mistral", providersFilter) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MISTRAL_API_KEY")))
{
    if (verbose) Console.WriteLine(">>> Mistral");
    try
    {
        var baseUrl = Environment.GetEnvironmentVariable("MISTRAL_BASE_URL") ?? "https://api.mistral.ai/v1/";
        var options = new MistralOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY")!,
            EmbeddingModel = Environment.GetEnvironmentVariable("MISTRAL_EMBEDDING_MODEL") ?? "mistral-embed",
            BaseUrl = baseUrl.TrimEnd('/') + "/"
        };
        using var http = CreateHttpClient(verbose, options.BaseUrl!, "Mistral", ("Authorization", "Bearer " + options.ApiKey));
        var provider = new MistralEmbeddingProvider(options, http);
        var result = provider.Embed("user:login");
        ValidateAndPrint("Mistral", result);
        RunFullPipelineAndPrint(provider, "Mistral");
        anyOk = true;
    }
    catch (MistralRateLimitException ex)
    {
        PrintRateLimit("Mistral", ex.RetryAfterSeconds, ex.ResponseBody);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        PrintRateLimit("Mistral", null, null);
    }
    catch (Exception ex) { Console.WriteLine($"  [FAIL] Mistral: {ex.Message}"); }
}

Console.WriteLine();
if (!anyOk)
{
    Console.WriteLine("Hiçbir sağlayıcı için env set edilmemiş. .env dosyasında en az bir sağlayıcının key'ini set edin (bkz. .env.example).");
    Environment.Exit(1);
}

Console.WriteLine("----------------------------------------");
Console.WriteLine("  [OK] En az bir sağlayıcı doğrulandı. Intentum entegrasyonu çalışıyor.");
Console.WriteLine("----------------------------------------");

static void RunFullPipelineAndPrint(IIntentEmbeddingProvider provider, string label)
{
    var similarityEngine = new SimpleAverageSimilarityEngine();
    var model = new LlmIntentModel(provider, similarityEngine);
    var space = new BehaviorSpace()
        .Observe("user", "login.attempt")
        .Observe("user", "login.success");
    var intent = model.Infer(space);
    if (intent is null) throw new InvalidOperationException("Infer null döndü.");
    if (string.IsNullOrEmpty(intent.Name)) throw new InvalidOperationException("Intent adı boş.");
    if (intent.Confidence is null || intent.Confidence.Score < 0.0 || intent.Confidence.Score > 1.0)
        throw new InvalidOperationException($"Confidence geçersiz: {intent.Confidence?.Score}");
    var policy = new IntentPolicy()
        .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
        .AddRule(new PolicyRule("Observe", _ => true, PolicyDecision.Observe));
    var decision = intent.Decide(policy);
    Console.WriteLine($"  [OK] {label} tam pipeline: Intent='{intent.Name}', Confidence={intent.Confidence.Level} ({intent.Confidence.Score:F4}), Decision={decision}.");
}

static void ValidateAndPrint(string name, IntentEmbedding? result)
{
    if (result == null) throw new InvalidOperationException("Cevap null.");
    if (result.Source != "user:login") throw new InvalidOperationException($"Source beklenen 'user:login', gelen '{result.Source}'.");
    if (result.Score < 0.0 || result.Score > 1.0) throw new InvalidOperationException($"Score [0,1] aralığında olmalı, gelen: {result.Score}.");
    Console.WriteLine($"  [OK] {name}: istek atıldı, cevap doğrulandı (Score={result.Score:F4}).");
}

static void PrintRateLimit(string label, double? retryAfterSeconds, string? responseBody)
{
    var isQuota = !string.IsNullOrWhiteSpace(responseBody) &&
                  (responseBody.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase) ||
                   responseBody.Contains("quota", StringComparison.OrdinalIgnoreCase));
    if (isQuota)
    {
        Console.WriteLine($"  [SKIP] {label}: insufficient_quota / quota (kota yok, bu sağlayıcı atlanıyor).");
        return;
    }
    Console.WriteLine($"  [FAIL] {label}: Rate limit (429). Sağlayıcı 5 kez yeniden denedi.");
    if (!string.IsNullOrWhiteSpace(responseBody))
    {
        var preview = responseBody.Trim().Length > 400 ? responseBody.Trim()[..400] + "…" : responseBody.Trim();
        Console.WriteLine($"  {label} yanıtı: {preview}");
    }
    if (retryAfterSeconds is > 0 and <= 86400)
    {
        var ts = TimeSpan.FromSeconds(retryAfterSeconds.Value);
        var msg = ts.TotalMinutes >= 1
            ? $"  Sunucu önerisi: {(int)ts.TotalMinutes} dk {ts.Seconds} sn bekleyin."
            : $"  Sunucu önerisi: {(int)retryAfterSeconds.Value} saniye bekleyin.";
        Console.WriteLine(msg);
    }
    else
        Console.WriteLine("  Genellikle 1–2 dakika bekleyip tekrar deneyin.");
}

static void PrintOpenAIRateLimit(double? retryAfterSeconds, string? responseBody)
{
    PrintRateLimit("OpenAI", retryAfterSeconds, responseBody);
    Console.WriteLine("  (Intentum rate limit = uygulama kotası; 429 = API kotası. Kullanım 0 ise: Billing / insufficient_quota kontrol edin.)");
}

static HttpClient CreateHttpClient(bool verbose, string baseUrl, string label, (string name, string value) authHeader)
{
    HttpMessageHandler handler = verbose
        ? new VerboseHttpHandler(new HttpClientHandler(), label)
        : new HttpClientHandler();
    var client = new HttpClient(handler);
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add(authHeader.name, authHeader.value);
    return client;
}

file sealed class VerboseHttpHandler(HttpMessageHandler inner, string label) : DelegatingHandler(inner)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri?.ToString() ?? request.RequestUri?.PathAndQuery ?? "";
        Console.WriteLine($"  --- [{label}] Request: {request.Method} {uri}");
        foreach (var h in request.Headers)
            Console.WriteLine($"      {h.Key}: {(h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ? "***" : string.Join(", ", h.Value))}");
        if (request.Content != null)
        {
            var reqBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"      Body: {reqBody}");
            request.Content = new StringContent(reqBody, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType ?? "application/json");
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        Console.WriteLine($"  --- [{label}] Response: {(int)response.StatusCode} {response.ReasonPhrase}");
        var resBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"      Body: {(resBody.Length > 800 ? resBody[..800] + "…" : resBody)}");
        response.Content = new StringContent(resBody, Encoding.UTF8, response.Content.Headers.ContentType?.MediaType ?? "application/json");
        return response;
    }
}
