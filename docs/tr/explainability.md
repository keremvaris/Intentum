# Intent açıklanabilirliği

Intentum **sinyal tabanlı açıklanabilirlik** (hangi davranışların ne kadar katkı yaptığı) ve isteğe bağlı **reasoning** (model veya kuraldan kısa metin) sunar.

## Sinyal katkıları

[IntentExplainer](api.md) **sinyal katkılarını** hesaplar: intent'teki her sinyal (davranış boyutu) için toplam ağırlıktaki pay (yüzde). Katkıya göre sıralı `(Source, Description, Weight, ContributionPercent)` listesi almak için `GetSignalContributions(intent)` kullanın.

**Kullanım:** "Model bu intent'i neden çıkardı?" — en yüksek N sinyali (örn. `user:login.failed`, `user:retry`) ve yüzdelerini gösterin.

## İnsan tarafından okunabilir açıklama

`GetExplanation(intent, maxSignals)` tek bir metin döndürür: intent adı, güven düzeyi/skoru, en çok katkı yapan sinyaller ve **reasoning** (`Intent.Reasoning` set edilmişse).

- **Reasoning:** Intent modeliniz (örn. Claude mesaj modeli veya kural tabanlı model) `Intent.Reasoning` set ettiğinde, açıklayıcı bunu açıklamaya ekler. Örnek: GreenwashingIntentModel `Reasoning`'i kısa gerekçeye (örn. "N sinyal; ağırlıklı skor X → IntentName") set eder. LLM tabanlı modeller kısa bir "çünkü …" cümlesi set edebilir.
- **Sinyal cümleleri:** En yüksek N sinyal için açıklayıcı bunları zaten okunabilir metne çevirir (örn. "user:login.failed (%25); user:retry (%20)"). UI veya log katmanında `Description` (actor:action) ile insan cümlesi eşlemesi (örn. "user:login.failed" yerine "Başarısız giriş") yaparak genişletebilirsiniz.

## Intent ağacı (karar ağacı)

**IIntentTreeExplainer** policy yolunu **karar ağacı** olarak oluşturur: hangi kural eşleşti, sinyal düğümleri, intent özeti. Policy’nin neden Allow/Block döndüğünü ağaç formunda göstermek (denetim veya UI) için kullanın.

- **IntentTreeExplainer.ExplainTree(intent, policy, behaviorSpace?)** **IntentDecisionTree** (IntentSummary, SignalNodes, MatchedRule) döndürür.
- Sample Web: `POST /api/intent/explain-tree` (infer ile aynı body) ağaç JSON’unu döndürür.

Kurulum ve seçenekler için [Gelişmiş Özellikler – Intent Tree](advanced-features.md#intent-tree) bölümüne bakın.

## Örnek kullanım

Sample.Web: `POST /api/intent/explain` sinyal katkılarını ve açıklama metnini döndürür. Yanıt en önemli sinyalleri ve model sağlıyorsa reasoning'i içerir. Karar ağacı için `POST /api/intent/explain-tree` kullanın.

## Özet

| Özellik           | Nerede                         | Kullanım |
|-------------------|--------------------------------|----------|
| Sinyal katkıları   | `IIntentExplainer.GetSignalContributions` | Hangi davranışlar ne kadar katkı yaptı |
| Açıklama metni    | `IIntentExplainer.GetExplanation` | Tek metin: ad, güven, en önemli sinyaller, reasoning |
| Intent ağacı      | `IIntentTreeExplainer.ExplainTree` | Karar ağacı: eşleşen kural, sinyal düğümleri; Sample: `POST /api/intent/explain-tree` |
| Reasoning         | `Intent.Reasoning` (model tarafından set edilir) | Kural veya LLM'den kısa "neden"; varsa açıklamaya eklenir |
