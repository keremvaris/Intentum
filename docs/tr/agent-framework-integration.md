# Agent framework entegrasyonu (Faz 4)

Intentum, agent framework'leri (Semantic Kernel, LangChain, AutoGen, CrewAI, LlamaIndex) için **intent doğrulama / guardrail** katmanı olarak kullanılabilir. Bu sayfa, davranış olaylarını Intentum'a besleyen ve agent aksiyonlarına politika kararları uygulayan bir **adapter** veya **middleware** oluşturmayı özetler.

---

## Amaç

- **Girdi:** Agent framework olayları yayar (örn. "kullanıcı X sordu", "Y aracı çağrıldı", "adım tamamlandı"). Bunları Intentum **BehaviorSpace** (actor:action) ile eşleyin.
- **Intentum:** `model.Infer(space)` ve `intent.Decide(policy)` çalıştırın.
- **Çıktı:** **Kararı** (Allow, Block, Observe vb.) agent'ın devam etmesi, engellenmesi veya eskalasyonu için kullanın; isteğe bağlı **intent adı** ve **güven** ile yönlendirme veya loglama yapın.

---

## Adapter sorumlulukları

1. **Olay eşleme:** Framework'e özgü olayları (örn. LangChain "tool call", Semantic Kernel "step") `BehaviorEvent(actor, action, occurredAt)`'e çevirin. Örnek: actor = "agent", action = "tool.search"; veya actor = "user", action = "message.submit".
2. **Uzay yaşam döngüsü:** Oturum veya konuşma başına bir `BehaviorSpace` oluşturun veya yeniden kullanın; her olay için `space.Observe(actor, action)` çağırın.
3. **Inference:** Karar gerektiğinde (örn. bir aracı çalıştırmadan önce veya N adım sonrası) `model.Infer(space)` ve `intent.Decide(policy)` çağırın.
4. **Politika:** Intent + güveni Allow/Block/Observe ile eşleyen bir `IntentPolicy` tanımlayın (örn. güven düşük ve sinyaller kötüye kullanım gösteriyorsa Block).

---

## Örnek arayüz (sözde kod)

```csharp
// Kavramsal adapter arayüzü (framework başına uygulayın)
public interface IIntentumAgentAdapter
{
    void RecordEvent(string actor, string action);
    (Intent Intent, PolicyDecision Decision) InferAndDecide();
}
```

Uygulama: bir `BehaviorSpace` tutun, `RecordEvent`'i `space.Observe(actor, action)` olarak, `InferAndDecide`'i `model.Infer(space)` + `intent.Decide(policy)` olarak uygulayın.

---

## Hedef framework'ler

- **Semantic Kernel (.NET):** Kernel adımlarından önce/sonra çalışan middleware veya filtre; adımları actor:action'a eşleyin, Intentum çağırın, engelleyin veya izin verin.
- **LangChain / LlamaIndex (Python):** Intentum çalıştıran küçük bir .NET veya HTTP servisini çağıran Python tarafı istemci gerekir veya Intentum'un Python portu.

---

## Özet

| Adım           | Eylem |
|----------------|--------|
| Olay eşleme    | Framework olayları → BehaviorEvent(actor, action) |
| Uzay           | Oturum başına bir BehaviorSpace; her olay için Observe() |
| Inference      | model.Infer(space); intent.Decide(policy) |
| Kararı kullan  | Allow → devam; Block → dur veya eskalasyon; Observe → logla ve devam et |

Somut bir adapter (örn. Semantic Kernel için) Intentum org'da ayrı bir repo veya örnek olarak eklenebilir. Bu doküman Faz 4 "agent framework middleware" spesifikasyonu niteliğindedir.
