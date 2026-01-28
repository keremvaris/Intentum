# Intentum.AI.Caching.FusionCache

FusionCache entegrasyonu için paket. Şu anda geliştirme aşamasında.

## Durum

FusionCache paketi entegre edilmeye çalışılıyor. Paket restore edildi ancak namespace'ler henüz çözülemedi.

## Plan

1. FusionCache paketinin doğru namespace'lerini belirle
2. `FusionCacheEmbeddingCache` implementasyonunu tamamla
3. Extension metodları düzelt
4. Redis desteği ekle
5. Test ve dokümantasyon

## Notlar

- FusionCache paketi: `ZiggyCreatures.FusionCache` v2.5.0
- Extension metodlar `Microsoft.Extensions.DependencyInjection` namespace'inde olmalı
- `IFusionCache` ve diğer tipler `ZiggyCreatures.FusionCache` namespace'inde olmalı
