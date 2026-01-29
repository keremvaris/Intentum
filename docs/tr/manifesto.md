# Intentum Manifestosu

**AI Çağı için Niyet Odaklı Geliştirme**

---

## 1. Yazılım artık deterministik değil.

Modern yazılım sistemleri artık öngörülebilir adımların bir dizisi değil.
Olasılıksal, adaptif ve bağlam, geçmiş ve belirsizlikten etkileniyorlar.

Yine de onları doğrusal senaryolar gibi test ediyoruz.

İşte sorun da bu.

---

## 2. Davranış niyet değildir.

Geleneksel geliştirme şunu sorar:

*Kullanıcı ne yaptı?*

Intentum ise olaya böyle yaklaşır:

*Kullanıcı ne yapmaya çalışıyordu?*

Eylemler belirti, niyet ise neden.

Davranışa takılıp kalan sistem, anlama kör kalır.

---

## 3. Senaryolar kırılgandır. Niyet dayanıklıdır.

BDD senaryoları şunlarda kırılır:

- akışlar değişir
- özellikler evrilir
- AI kararları kayar
- edge case'ler çoğalır

Niyet kırılmaz.

Niyet şunlara dayanır:

- retry'lar
- hatalar
- kısmi başarı
- alternatif yollar

Senaryolar yolu anlatır.
Niyet yönü anlatır.

---

## 4. Testler senaryo değil, uzay tanımlamalıdır.

Test bir hikâye değil.

Test bir davranış uzayı:

- sinyaller
- olasılıklar
- güven
- tolerans

Intentum doğruluğu boolean değil, dağılım olarak ele alır.

Çünkü akıllı sistemlerde doğruluk asla mutlak değildir.

---

## 5. AI sistemleri "Given–When–Then" ile doğrulanamaz.

Given–When–Then şunu varsayar:

- net girdiler
- deterministik geçişler
- ikili sonuçlar

AI üçünü de bozar.

Intentum şunun yerini alır:

- **Given** → Gözlemlenen sinyaller
- **When** → Davranış evrimi
- **Then** → Niyet güveni

Bu bir refactor değil; paradigma kayması.

---

## 6. Niyet yeni sözleşmedir.

API'ler önce fonksiyonları açtı.
Sonra event'leri.
Şimdi niyeti açmalılar.

Niyet şunun olur:

- sınır
- beklenti
- değişmez

Davranış değişse bile niyet hizalı kalıyorsa sistem doğrudur.

---

## 7. Başarısızlıklar ihlal değil, sinyaldir.

Niyet Odaklı Geliştirme'de:

- başarısızlıklar veridir
- retry'lar bilgidir
- anomaliler bağlamdır

Sistemler sapmadan öğrenmeli, sapma yüzünden çökmemelidir.

---

## 8. Kontrol için değil, anlama için tasarlıyoruz.

Adaptif sistemlerde kontrol illüzyondur.

Anlama değildir.

Intentum sistemlerin:

- anlam çıkarmasına
- belirsizlik altında akıl yürütmesine
- kesinlikle değil güvenle hareket etmesine

yardım etmek için vardır.

Intentum BDD'nin alternatifi değildir.
AI sisteme girdiğinde BDD'nin evrildiği şeydir.

---

## Tek cümlelik felsefe

> **Yazılım olaylara göre değil, niyete göre yargılanmalıdır.**

Özet kurallar için bkz. [Intentum Canon](intentum-canon.md) (10 ilke).

---

## Intentum ne zaman sizin için değil?

Intentum bilinçli biçimde belli bir görüşe sahip.

Sisteminiz:

- deterministik
- statik
- kural tabanlı

ise ihtiyacınız olmayabilir.

Ama sisteminiz şunları içeriyorsa:

- AI
- adaptif mantık
- olasılıksal kararlar
- insan belirsizliği

Niyet Odaklı Geliştirme o zaman seçenek değil; kaçınılmaz olur.
