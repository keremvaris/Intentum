# Bu Özellikler Ne İşe Yarar — Basit Rehber

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum'daki her özelliğin (Intent Timeline, Intent Tree, Policy Store, Multi-Stage Model vb.) ne işe yaradığını sade bir dille özetler. "Bu özellik tam olarak ne yapıyor?" sorusuna hızlı yanıt arıyorsanız doğru yerdesiniz.

Intentum’a yeni biri için: her özelliğin ne işe yaradığı, sade bir dille.

---

## Intent Timeline

**Ne:** Belirli bir kullanıcı (veya oturum) için “zaman içinde niyeti ne oldu?” sorusunun **geçmişi** — kronolojik liste.

**Neden işe yarar:**  
Intent sonuçlarını isteğe bağlı bir **entity id** (örn. kullanıcı id, oturum id) ile kaydedersin. Sonra “Bu entity’nin niyeti zamanla nasıl değişti?” diye sorabilirsin — örn. Pazartesi: Düşük güven → Observe; Salı: Yüksek → Allow.  
**Destek** (“bu kullanıcı engellenmeden önce ne yaptı?”), **denetim** ve **dashboard**’lar (örn. kullanıcı başına “zaman içinde intent” grafiği) için kullanılır.

**Tek cümle:** *“Bu kullanıcının zaman içindeki intent geçmişini göster.”*

---

## Intent Tree

**Ne:** Sistemin neden Allow, Block, Observe vb. verdiğinin **açıklaması** — **karar ağacı** biçiminde: hangi kural eşleşti, hangi sinyaller (davranışlar) katkı yaptı.

**Neden işe yarar:**  
Kullanıcılar ve denetçiler kararın **neden** verildiğini bilmek ister. Ağaç yolu gösterir: “X kuralı eşleşti çünkü güven Yüksek’ti ve Y sinyali Z ağırlığındaydı.”  
**Şeffaflık**, **uyumluluk**, **destek** (“neden engellendim?”) ve **debug** (kuralları ince ayar) için kullanılır.

**Tek cümle:** *“Bu kararın neden verildiğini göster (hangi kural, hangi sinyaller).”*

---

## Context-Aware Policy

**Ne:** Kararların sadece **mevcut intent**’e değil, **bağlama** (context) da bağlı olduğu policy: örn. sunucu yükü, bölge, son intent’ler (“aynı intent 3 kez üst üste”) veya özel veri.

**Neden işe yarar:**  
Bazen doğru karar duruma bağlıdır: örn. “Yük %80’in üzerindeyse Block” veya “Kullanıcı aynı intent’i üç kez aldıysa escalate (takılmış olabilir).”  
Context = **yük**, **bölge**, **son intent’ler**, **özel anahtar-değer**.  
**Uyarlanabilir davranış** ve **operasyonel kurallar** (örn. aşırı yük, bölgeye göre A/B) için kullanılır.

**Tek cümle:** *“Kararı sadece intent’e değil, yüke, bölgeye ve son geçmişe göre ver.”*

---

## Policy Store

**Ne:** Policy’lerin **kod yerine JSON (veya dosya)** ile tanımlanması. Kuralları (koşul, karar) bu dosyada **düzenleyebilirsin**; uygulama **yeniden deploy etmeden** dosyayı tekrar yükleyebilir (hot-reload).

**Neden işe yarar:**  
Geliştirici olmayan personel (örneğin operasyon veya uyumluluk ekipleri) kuralları doğrudan güncelleyebilir. Örnek: “Güven Low ise Block” kuralı, “Güven Low veya sinyal sayısı 10'dan büyükse Block” şeklinde değiştirilebilir. Bu güncelleme için kod değişikliği veya yeni bir deploy gerekmez.  
**Hızlı kural güncellemesi** ve **düşük kod** policy yönetimi için kullanılır.

**Tek cümle:** *“Policy kurallarını dosyada değiştir; kod deploy’a gerek yok.”*

---

## Behavior Pattern Detector

**Ne:** **Intent geçmişini** analiz ederek **pattern**’ler (örn. “Pazartesi günleri çok Block”) ve **anomaliler** (örn. “Block oranında ani artış” veya “alışılmadık güven dağılımı”) bulur.

**Neden işe yarar:**  
Zamanla çok fazla intent verisi birikir. Bu özellik **pattern**’leri görmeni ve **anomalileri** işaretlemeni sağlar — örn. izleme, policy ince ayarı veya kötüye kullanım tespiti.  
**Dashboard**, **uyarılar** ve **model/policy iyileştirme** için kullanılır.

**Tek cümle:** *“Intent geçmişinde pattern ve anomali bul.”*

---

## Multi-Stage Model

**Ne:** Birden fazla intent modelini **zincir** halinde, **güven eşikleri** ile kullanma. Önce ilkini dene (örn. hızlı/ucuz kurallar); güven eşiğin altındaysa bir sonrakini (örn. hafif LLM); hâlâ düşükse sonuncuyu (örn. ağır LLM).

**Neden işe yarar:**  
Ağır modeller yavaş ve pahalı. Multi-stage onları **sadece gerektiğinde** (düşük güven) kullanır. Trafiğin çoğu kurallar veya ucuz modelle hallolur.  
**Maliyet** ve **gecikme** azaltma, zor vakalarda kaliteyi kaybetmeden.

**Tek cümle:** *“Önce ucuz/hızlı modeli kullan; pahalı modeli sadece güven düşükken kullan.”*

---

## Scenario Runner

**Ne:** **Önceden tanımlı senaryoları** (örn. “kullanıcı login, login, login, sonra submit yapar”) intent modeli ve policy üzerinden **çalıştırma**. Senaryo başına sonuç (örn. Allow/Block) alırsın.

**Neden işe yarar:**  
Sistemin beklediğin gibi davrandığını **test** etmek istersin: “Biri X yaparsa Block vermeliyiz.” Scenario Runner bir sürü böyle senaryoyu tek seferde çalıştırır — **regresyon testleri**, **demolar** ve **doğrulama** için.

**Tek cümle:** *“Tanımlı davranış senaryolarını çalıştır, Allow/Block gör (test ve demo).”*

---

## Stream (Gerçek zamanlı intent stream)

**Ne:** **Gelen davranış olaylarını** hepsini belleğe almadan **akış** gibi işleme (konveyör bandı gibi). **Batch**’ler halinde olay alırsın; her batch için intent çıkarıp policy uygularsın.

**Neden işe yarar:**  
Gerçek zamanlı pipeline’larda (mesaj kuyruğu, event hub vb.) olaylar “bitmez” — sürekli gelir. Stream bu olayları **batch’ler halinde tüketip işlemeni** sağlar; hepsini bellekte tutmana gerek kalmaz.  
**Worker**’lar, **event-driven** uygulamalar ve **yüksek hacimli** pipeline’lar için kullanılır.

**Tek cümle:** *“Davranış olaylarını tek seferde değil, sürekli akış (batch’ler) olarak işle.”*

---

## Playground

**Ne:** **Aynı** girdi (aynı olay listesi) üzerinde **farklı intent modellerinin** nasıl davrandığını **karşılaştırma**. Tek istekte olayları ve model adlarını (örn. “Default”, “Mock”) gönderirsin; **model başına** intent ve karar döner.

**Neden işe yarar:**  
Birden fazla modelin (kural tabanlı, Mock, gerçek LLM) olabilir. Playground “bu girdi için her model ne döndürüyor?” sorusunu yanıtlar — **debug**, **karşılaştırma** ve **doğru modeli seçme** için.

**Tek cümle:** *“Aynı olaylarda farklı modelleri karşılaştır (model başına intent + karar).”*

---

## Playground: Arayüz

**Şu an:** Sample Web’de **Playground** bölümü var (Örnekler → Playground): olayları girin veya hazır senaryo seçin, karşılaştırmak istediğiniz modelleri işaretleyin (Default, Mock), **Karşılaştır**’a tıklayın; model başına niyet adı, güven düzeyi, skor ve karar tabloda görünür. API: `POST /api/intent/playground/compare`.

---

## Özet tablo

| Özellik | Tek cümle |
|--------|------------|
| **Intent Timeline** | Bu kullanıcının zaman içindeki intent geçmişini göster. |
| **Intent Tree** | Bu kararın neden verildiğini göster (kural + sinyaller). |
| **Context-Aware Policy** | Kararı intent + yük, bölge, son geçmişe göre ver. |
| **Policy Store** | Policy kurallarını dosyada değiştir; kod deploy yok. |
| **Behavior Pattern Detector** | Intent geçmişinde pattern ve anomali bul. |
| **Multi-Stage Model** | Önce ucuz model; pahalı model sadece gerektiğinde. |
| **Scenario Runner** | Tanımlı senaryoları çalıştır, Allow/Block gör (test/demo). |
| **Stream** | Olayları sürekli akış (batch) olarak işle. |
| **Playground** | Aynı olaylarda farklı modelleri karşılaştır (şu an API; arayüz eklenebilir). |

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Gelişmiş özellikler](advanced-features.md) veya [API Referansı](api.md).
