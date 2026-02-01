# Blazor 5 Demo Senaryosu – Detaylı Spesifikasyonlar

Bu belge, Blazor örnek projesinde uygulanacak beş somut demo senaryosunun akışını, event listelerini, görsel metinlerini ve politika eşiklerini tek referans olarak tanımlar. Her demo "Gözlemle → Tahmin Et → Karar Ver" üçlüsünü sektöre özgü bir acı noktasıyla gösterir.

---

## Demo 1: Finans Dolandırıcılığı Senaryosu

**Konsept:** "Yaklaşan tehlikeyi tek bir olayda değil, birkaç dakika içinde gerçekleşen olaylar zincirinin oluşturduğu davranış modelinde görün."

### Demo Akışı

**Sahne kurulumu:** Demo arayüzü "Ahmet K." adlı bir kullanıcının banka hesabı kontrol panelini gösterir. Görsel: Normal hesap aktivitesi gösteren sakin bir zaman çizelgesi.

**Olaylar zinciri (Gözlemle – Observe):** Senaryo başlatıldığında aşağıdaki `BehaviorEvent`'ler dakikalar içinde simüle edilir.

| # | Metadata / Not | Eylem (Action) |
|---|-----------------|----------------|
| 1 | IP: 105.12.34.56 (yeni ülke) | Login |
| 2 | — | ChangeContactEmail |
| 3 | — | HighValueTransfer (maksimum limit) |
| 4 | — | RequestNewCardExpressShipping |

**Niyet çıkarımı (Tahmin et – Infer):** Her olaydan sonra bir `ChainedIntentModel` çalıştırılır.

- **RuleBasedIntentModel (ilk kontrol):** ChangeContactEmail sonrası HighValueTransfer kuralını tetikler → **FlagForReview** niyeti.
- **LlmIntentModel (davranış analizi):** Tüm olay dizisini bir bütün olarak gömme (embedding). TimeDecay motoru ile yakın zamanlı olaylar daha ağırlıklı. "Account Takeover for Fraudulent Transfer" niyeti ile yüksek benzerlik. **Confidence: %94**.

**Karar ve görselleştirme (Karar ver – Decide):**

- **Görsel 1 (Coğrafi harita):** Login IP'si kırmızıyla, kullanıcının normal lokasyonu yeşille yanar.
- **Görsel 2 (Skor grafiği):** Zaman çizelgesinin üzerinde her olayla birlikte yükselen "Fraud Risk Score" çizgisi. LlmIntentModel devreye girdiğinde skorun aniden sıçraması vurgulanır.
- **Karar:** IntentPolicy, güven skoru >%90 ise **Block** ve **Escalate** kararını verir.
- **Bildirim:** Ekranda büyük bir **"İŞLEM DURDURULDU & GÜVENLİK EKİBİNE BİLDİRİLDİ"** bildirimi belirir.

---

## Demo 2: Hesap Ele Geçirme Saldırısı (ATO)

**Konsept:** "Parçalar tehlikeli görünmeyebilir, ancak Intentum onları bir araya getirip resmi görebilir."

### Demo Akışı

**Olaylar:** Her biri başlı başına meşru olabilecek, hızlı bir olay akışı simüle edilir (hepsi aynı `BehaviorSpace`'e):

1. **FailedLogin** (yanlış şifre)
2. **PasswordResetRequest**
3. **EmailAccessFromNewDevice** (reset linkine tıklama)
4. **Login** (yeni cihaz, başarılı)
5. **ProfileUpdate** (2FA telefon numarası kaldırıldı)

**Davranış uzayı ve modelleme:** LlmIntentModel bu olaylar dizisini "Credential Stuffing & Account Hijacking" niyeti için eğitilmiş vektör uzayında değerlendirir. Kısa zaman dilimindeki yoğun, sıralı aktivite modeli **%88** güven skoru üretir.

**Görselleştirme:** Ekran ikiye bölünür.

- **Sol:** Olayların kronolojik listesi.
- **Sağ:** Her olayla birlikte güncellenen, büyüyen bir Intent kartı: **Intent.Name**, **Confidence** yüzdesi, modelin çıkarım yaparken dikkate aldığı anahtar kelimeler: "reset", "new device", "2fa removal".

---

## Demo 3: İçeriden Gelen Tehdit (Insider Threat)

**Konsept:** "Intentum normali bilir, normallikten sapmayı niyet değişikliğinin erken işareti olarak yorumlar."

### Demo Akışı

**Baz çizgisi oluşturma:** Demo "Merve B." adlı bir çalışanın 30 günlük tipik davranışını gösteren bir grafikle başlar: Sabah 9–18 mesai, belirli dahili sistemlere erişim, haftada ~100 dosya indirme.

**Anomali senaryosu:** İşten çıkarılma öncesindeki iki gün simüle edilir:

1. **MassFileDownload** (gece yarısı, normalin 10 katı)
2. **AccessToUnusualDatabase** (projesi olmayan müşteri veritabanı)
3. **AttemptToUsePrivilegedAPIs** (yetki hatası)

**Tahmin ve politika:** IIntentModel bu davranışları çalışanın tarihsel baz çizgisi vektörüyle karşılaştırır. Büyük sapma "Data Exfiltration Preparation" niyetine yüksek puan verir. IntentPolicy: güven skoru eşiğini aştığında **RequireAuth** (ek kimlik doğrulama) ve yöneticiye **Warn** kararı.

**Görselleştirme:** Yan yana iki çubuk grafik: Solda "Normal Aktivite Baz Çizgisi", sağda "Son 48 Saat". Anormal indirme ve erişim çubukları kırmızıyla parlarken, ekrana bir Warn kararı simülasyonu gelir.

---

## Demo 4: Sıfır-Gün Saldırı Davranışı

**Konsept:** "İmza henüz yok, ancak kötü niyetli davranış modeli değişmedi."

### Demo Akışı

**Bilinmeyen saldırı:** Yeni, imza tabanlı güvenlik duvarlarının tanımadığı bir "yatay hareket" saldırısı simüle edilir:

- **PortScan**
- **ExploitAttempt** (bilinen bir CVE'siz)
- **LateralMoveToServerB**

**Davranış tabanlı eşleme:** ChainedIntentModel çalışır.

- **RuleBasedIntentModel** belirli exploit imzasını tanımadığı için düşük puan verir.
- **LlmIntentModel** olayların gömme vektörünü hesaplar; veritabanındaki "Lateral Movement & Discovery" ve "Ransomware Precursor Activity" gibi bilinen kötü niyetli davranış kümeleriyle yüksek benzerlik tespit eder.

**Görsel vurgu:** Ekran saldırı adımlarını gösteren bir ağ diyagramına odaklanır. Intentum niyeti belirlediği anda diyagramın üzerinde büyük, sarı bir uyarı banner'ı belirir:

**"⚠ BİLİNMEYEN İMZA, ANCAK BİLİNEN DAVRANIŞ: 'Ağ Keşfi & Fidye Yazılımı Hazırlığı' %82 Güven"**

Yanında geçmişte tespit edilmiş benzer davranış modellerinin sayısı gösterilir.

---

## Demo 5: API/Web Trafiği Anormalliği

**Konsept:** "Intentum trafiği sadece hacim olarak değil, niyet olarak da sınıflandırır ve buna göre tepki verir."

### Demo Akışı

**Normal trafik:** Canlı bir "Requests/Second" grafiği kullanıcı benzeri istikrarlı bir trafik modeli gösterir.

**Bot saldırısı başlar:** Grafik aynı IP havuzundan gelen ve aynı API endpoint'ini (`/api/v1/validate-coupon`) hedef alan dik bir çıkış gösterir. BehaviorEvent'ler "API_CALL" olarak kaydedilir.

**Yüksek hızlı işleme ve oran sınırlama:** Intentum akış işlemcisi bu olayları toplar. **RuleBasedIntentModel** saniyedeki istek eşiğini aşan her IP için "AutomatedScanOrAttack" niyetini çıkarır. İlgili IntentPolicy hemen **RateLimit** kararı döndürür.

**Entegre karar ve geri bildirim:** Demo Intentum'un kararının sistem üzerindeki etkisini gerçek zamanlı gösterir. **IRateLimiter** devreye girer; saldırgan IP'lere **429 Too Many Requests** yanıtı dönmeye başlar.

- **Görsel 1:** Trafik grafiği: ani artış, ardından Intentum'un müdahalesiyle düşüş.
- **Görsel 2:** Harita: saldırı kaynaklarını kırmızı noktalarla, engellenen trafik akışlarını kırmızı çizgilerle gösterir.

---

## Uygulama İçin Event ve Politika Eşlemesi

| Demo | BehaviorEvent Actions (Actor örn. user/employee/IP) | RuleBased kuralları (özet) | Politika (skor eşiği) |
|------|-----------------------------------------------------|----------------------------|-------------------------|
| 1 | Login, ChangeContactEmail, HighValueTransfer, RequestNewCardExpressShipping | ChangeContactEmail + HighValueTransfer → FlagForReview | >0.90 → Block + Escalate |
| 2 | FailedLogin, PasswordResetRequest, EmailAccessFromNewDevice, Login, ProfileUpdate | (opsiyonel) | — |
| 3 | MassFileDownload, AccessToUnusualDatabase, AttemptToUsePrivilegedAPIs | Baz çizgisi sapması + intent | RequireAuth, Warn |
| 4 | PortScan, ExploitAttempt, LateralMoveToServerB | Bilinen imza yok → düşük; fallback LLM | — |
| 5 | API_CALL (metadata: path, IP) | Saniyedeki istek eşiği → AutomatedScanOrAttack | RateLimit; IRateLimiter → 429 |

---

## Intentum Bileşenleri Özeti

- **Demo 1:** ChainedIntentModel (RuleBasedIntentModel + LlmIntentModel), TimeDecaySimilarityEngine, IntentPolicy (Block, Escalate), BehaviorEvent.Metadata (IP, ülke).
- **Demo 2:** BehaviorSpace (sıralı olaylar), LlmIntentModel, Explain/Intent detay (anahtar kelimeler).
- **Demo 3:** Baz çizgisi vektörü (özel veya Analytics), IIntentModel, IntentPolicy (RequireAuth, Warn), Analytics pattern/anomaly.
- **Demo 4:** ChainedIntentModel (RuleBased düşük + LlmIntentModel fallback), reasoning metni "Bilinmeyen İmza, Bilinen Davranış", benzer geçmiş olay sayısı.
- **Demo 5:** RuleBasedIntentModel (rate eşiği), IntentPolicy RateLimit, IRateLimiter, BehaviorObservation veya benzeri event kaynağı.

Bu spesifikasyonlar [case-studies README](README.md) ile birlikte implementasyon için tek referans olarak kullanılabilir.

---

## Plana Dahil Ek Görev: Sol Menü Toggle Bug'ı

**Sorun:** Sol menü (sidebar) açılıp kapanmıyor, sürekli açık kalıyor.

**Sebep:** Uygulama `MainLayout` kullanıyor; sol menü `aside.dashboard-sidebar` ile tanımlı ve **hiç hamburger/toggle butonu yok**. Sidebar her zaman görünür. (`NavMenu.razor` içindeki checkbox toggler kullanılmıyor.)

**Hedef:** Sol menüyü açıp kapatabilmek.

**Önerilen düzeltme:**

1. **MainLayout.razor:** Sidebar'ı aç/kapa yapacak bir state ekle (`_sidebarOpen`). Üst satıra (sidebar-brand yanına veya `dashboard-body` içine) hamburger butonu ekle; tıklanınca `_sidebarOpen = !_sidebarOpen`. Sidebar `<aside>` sınıfına koşullu class ekle (örn. `@(_sidebarOpen ? "open" : "collapsed")`).
2. **dashboard.css:** `.dashboard-sidebar.collapsed` için genişliği 0 (veya `transform: translateX(-100%)`), overflow hidden; hamburger butonu stilleri. Mobilde (örn. `max-width: 768px`) varsayılanı `collapsed` yapıp hamburger ile açılabilir; masaüstünde isteğe bağlı collapse.
3. İsteğe bağlı: Sidebar kapalıyken içerik alanı tam genişlik; overlay veya gölge ile kapatılmış sidebar belirgin olsun.

Bu madde, 5 demo implementasyonu ile birlikte veya önce uygulanabilir.
