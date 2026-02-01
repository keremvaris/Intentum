# Clustering ve Experiments: kullanım senaryoları

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum.Clustering ve Intentum.Experiments için kullanım senaryolarının kısa özetini verir: intent gruplama, A/B test, politika ayarı, anomali tespiti. Bu paketlerin ne işe yaradığını merak ediyorsanız doğru yerdesiniz.

**Intentum.Clustering** ve **Intentum.Experiments** için **kullanım senaryolarının** kısa özeti. Tam API ve kurulum: [Gelişmiş Özellikler](advanced-features.md#intent-clustering), [Gelişmiş Özellikler — A/B deneyleri](advanced-features.md#ab-experiments).

---

## Intentum.Clustering

**Ne yapar:** Intent geçmişi kayıtlarını (örn. `IIntentHistoryRepository`'den) örüntüye (güven düzeyi + karar) veya güven skoru dilimlerine göre kümeler.

**Kullanım senaryoları:**

- **Kümeye göre politika:** Hangi kümelerin (örn. "High + Allow", "Low + Block") en sık görüldüğünü inceleyin; "Low + Block" hedef payda kalsın diye politika kurallarını veya eşikleri ayarlayın.
- **Anomali tespiti:** Yeni bir küme belirirse (örn. kısa pencerede çok sayıda "Medium + Observe") uyarı veya inceleme tetikleyin.
- **Dashboard:** Güven bandı ve karara göre intent dağılımını gösterin (örn. pasta grafik: Allow %60, Observe %25, Block %15).

**Nasıl:** `AddIntentClustering()` ile kaydedin, `IIntentClusterer` çözün, zaman penceresine göre kayıtları alın, `ClusterByPatternAsync` veya `ClusterByConfidenceScoreAsync` çağırın. Bkz. [Intent Clustering](advanced-features.md#intent-clustering).

---

## Intentum.Experiments

**Ne yapar:** Intent inference üzerinde A/B testi — birden çok varyant (model + politika), trafik bölümü (örn. %50 / %50), davranış uzaylarını deneyden geçirip uzay başına bir sonuç (varyant adı, intent, karar) alırsınız.

**Kullanım senaryoları:**

- **A/B intent modeli:** Aynı trafikte iki modeli karşılaştırın (örn. mevcut LlmIntentModel vs yeni model veya yeni embedding sağlayıcısı); varyant başına güven ve karar dağılımını ölçün.
- **A/B politika:** Aynı model çıktısında iki politikayı karşılaştırın (örn. mevcut vs daha sıkı Block kuralları); varyant başına Allow/Block/Observe ölçün.
- **Rollout:** Yeni model veya politikaya trafiği kademeli artırın (örn. %10 deneme, %90 kontrol); tam rollout kararı için deney sonuçlarını kullanın.

**Nasıl:** `IntentExperiment` oluşturup `AddVariant` (ad, model, politika, trafik %) ekleyin, `RunAsync(spaces)` çalıştırın. Bkz. [A/B Experiments](advanced-features.md#ab-experiments).

---

## Özet

| Paket                 | Kullanım özeti |
|-----------------------|-----------------|
| **Intentum.Clustering**  | Intent geçmişini örüntü veya skora göre grupla; politika ayarı, anomali tespiti, dashboard'lar. |
| **Intentum.Experiments**  | Model veya politikada A/B test; varyantları karşılaştır, yeni model/politika rollout. |

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Gelişmiş özellikler](advanced-features.md) veya [Senaryolar](scenarios.md).
