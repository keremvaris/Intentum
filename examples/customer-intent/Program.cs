// Intentum Example: Customer intent (purchase, info gathering, support)
// Run: dotnet run --project examples/customer-intent
// No API key needed (uses Mock embedding provider).

using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

// Policy: allow high-confidence; observe medium; route support to human when signals indicate support intent
var policy = new IntentPolicyBuilder()
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
    .Warn("LowConfidence", i => i.Confidence.Level == "Low")
    .Build();

Console.WriteLine("=== Intentum Example: Customer intent ===\n");

// Scenario 1: Purchase intent (browse, cart, checkout, pay)
var space1 = new BehaviorSpace()
    .Observe("user", "browse.category")
    .Observe("user", "cart.add")
    .Observe("user", "checkout.start")
    .Observe("user", "payment.submit");

var intent1 = intentModel.Infer(space1);
var decision1 = intent1.Decide(policy);

Console.WriteLine("Scenario 1 — Purchase (browse → cart → checkout → pay)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Info gathering (search, view, compare, no purchase)
var space2 = new BehaviorSpace()
    .Observe("user", "search.product")
    .Observe("user", "view.product")
    .Observe("user", "compare.product")
    .Observe("user", "view.faq");

var intent2 = intentModel.Infer(space2);
var decision2 = intent2.Decide(policy);

Console.WriteLine("Scenario 2 — Info gathering (search, view, compare, faq)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");
Console.WriteLine();

// Scenario 3: Support intent (contact, ticket, chat)
var space3 = new BehaviorSpace()
    .Observe("user", "contact.click")
    .Observe("user", "ticket.create")
    .Observe("user", "chat.start");

var intent3 = intentModel.Infer(space3);
var decision3 = intent3.Decide(policy);

Console.WriteLine("Scenario 3 — Support (contact, ticket, chat)");
Console.WriteLine($"  Confidence: {intent3.Confidence.Level} (score: {intent3.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision3}");
Console.WriteLine();

Console.WriteLine("Use intent name + confidence to route: purchase → checkout flow; info → content; support → human/chat.");
Console.WriteLine("See docs/en/domain-intent-templates.md for intent names and suggested actor:action.");