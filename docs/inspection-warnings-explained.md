# Rider Inspection Uyarıları: Neden 25 Dosya / 53 Uyarı Kalıyor?

Yapılan temizlikten sonra hâlâ görünen uyarıların çoğu **bilinçli olarak** dokunulmadığı için kalıyor. Kısa özet:

## 1. Public API / NuGet’e dokunmuyoruz

- **InconsistentNaming** (örn. `OpenAI` → `OpenAi`): Rider “Types and namespaces” kuralına göre “OpenAi” istiyor. Değiştirirsek **breaking change** olur; mevcut kullanıcılar `OpenAIOptions`, `AddIntentumOpenAI` vb. kırılır.
- **UnusedType.Global / UnusedMember.Global**: `AddIntentumOpenAI`, `AddIntentumClaude`, `RedisCachingExtensions` vb. **paket tüketicileri** tarafından kullanılıyor. Rider solution içinde extension çağrısı görmediği için “never used” diyor; gerçekte public API.

## 2. API / JSON sözleşmeleri

- **NotAccessedPositionalProperty**: Request/response DTO’lardaki property’ler JSON serileştirme için; kod içinde doğrudan okunmayabilir. Kaldırırsak API sözleşmesi bozulur.
- **ClassNeverInstantiated.Local**: Aynı şekilde JSON için kullanılan private record’lar; `new` ile açıkça oluşturulmasa da serileştirici kullanıyor.

## 3. CSharpErrors (InspectCode’da ERROR)

Bunlar çoğunlukla **cross-project / solution-wide** çözümleme kaynaklı: InspectCode bazı referansları çözemiyor, `ConfidenceLevel`, `IntentHistoryRecord` vb. “cannot resolve” diyor. `dotnet build` 0 uyarı veriyorsa gerçek derleme hatası yok; raporlamayı azaltmak için InspectCode’u `--no-swea` veya belirli projelerle sınırlayabilirsin.

## Ne yapıldı (gizlemeden temizlik)

- **Public API “unused” uyarıları:** Extension sınıfları ve metotları, interface üyeleri (örn. `IEmbeddingCache.Remove`/`Clear`, `IIntentClusterer.ClusterByPatternAsync`), `FromEnvironment` gibi paket tüketicileri tarafından kullanılan API’ler **`[UsedImplicitly]`** (JetBrains.Annotations) ile işaretlendi. Böylece Rider “never used” demiyor; uyarılar gizlenmiyor, doğru şekilde “dışarıdan kullanılıyor” olarak işaretleniyor.
- **`.editorconfig` ile severity düşürme kaldırıldı;** uyarılar gizlenmiyor.

## İstersen devamında

1. **InconsistentNaming (OpenAI → OpenAi):** İsimlendirmeyi C# kurallarına uydurmak için `OpenAI` → `OpenAi` (ve AzureOpenAI → AzureOpenAi) yapılabilir; bu **breaking change** olur, major sürümde yapılmalı.
2. **NotAccessedPositionalProperty / ClassNeverInstantiated:** JSON DTO’larda bu tipler/özellikler serileştirici tarafından kullanılıyor; gerekirse tiplere `[UsedImplicitly]` eklenebilir.
3. **InspectCode çıktısını filtrelemek:** Script’te `-e=ERROR` veya `--project=…` ile sadece istediğin severity/projeyi raporlayabilirsin.

Özet: **53 uyarı** büyük ölçüde “public API’ye dokunma” ve “DTO/JSON sözleşmesi” kararından kaynaklanıyor; build ve testler temiz. İstersen bir sonraki adımda hangi 25 dosyada ne tür uyarı kaldığını listeleyip, sadece güvenli olanları (ör. Redundant*, bazı SUGGESTION’lar) temizleyebiliriz veya DotSettings ile sadece sayıyı düşürecek ayarı ekleyebiliriz.
