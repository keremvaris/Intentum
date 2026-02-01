# Neden Niyet ≠ Log / Event?

**Bu sayfayı neden okuyorsunuz?** Bu sayfa "Zaten log tutuyoruz, event'ler var" itirazına yanıt verir: log/event ile niyet arasındaki farkı ve niyetin neden sadece dashboard'larla türetilemeyeceğini anlatır. Intentum'un neden "log + analytics" yerine niyet katmanı sunduğunu merak ediyorsanız doğru yerdesiniz.

Ve niyetin neden sadece dashboard'larla türetilemeyeceği.

---

## Yaygın itiraz

Intentum'u ilk kez görenlerin aklına genelde şunlar geliyor:

"Zaten log tutuyoruz."
"Event'ler var."
"Analytics halleder."

Hayır.

**Log ≠ Niyet.**

Aradaki fark kritik.

---

## 1. Loglar olguyu anlatır. Niyet anlamı anlatır.

Bir log şunu söyler:

- `login.failed`
- `ip.changed`
- `captcha.passed`
- `login.success`

Bunlar olgudur.

Ama şu soruya cevap vermezler:

*Bu davranış neden gerçekleşti?*

Intentum aynı event setini kullanır — ama bağlamdan *anlam* ve *niyet* çıkarır.

---

## 2. Event'ler atomiktir. Niyet ilişkiseldir.

Bir event:

- tekil
- zamansal
- bağlamsız

Niyet:

- ilişkiseldir
- örüntü arar
- anlam üretir

Örnekler:

- `login.failed` + retry = gürültü
- `login.failed` + retry + reset = kurtarma
- `login.failed` + retry + ip.change = risk

Aynı event'ler. Farklı anlam.

---

## 3. Dashboard'lar geçmişe odaklanır, karara değil.

Analytics:

- geçmişi özetler
- KPI üretir
- trend gösterir

Karar anında devreye giremez.

Intentum:

- canlı sinyallerle çalışır
- belirsizliği kabul eder
- karar *öncesi* anlam üretir

**Dashboard "Ne oldu?" der.**  
**Niyet "Şimdi ne yapmalıyız?" der.**

---

## 4. Loglar pasiftir. Niyet aksiyoneldir.

Log:

- yazılır
- saklanır
- okunur

Niyet:

- hesaplanır
- skorlanır
- kararı besler

Dashboard ile bu kararı `if` ile veremezsiniz.

---

## 5. AI sistemleri event tabanlı akıl yürütmeyi bozar.

AI davranışı:

- tutarsız
- olasılıksal
- adaptif

Event tabanlı sistemler:

- determinizm varsayar
- sıraya takılır
- edge case'te çöker

Niyet tabanlı sistemler:

- toleranslıdır
- esnektir
- model değişimine dayanıklıdır.

---

## 6. Loglar lineer ölçeklenir. Niyet bilişsel ölçeklenir.

Log hacmi:

- arttıkça kaos artar

Niyet:

- sinyalleri sıkıştırır
- anlamsal yoğunluk üretir
- bilişsel yükü azaltır

Daha fazla log ≠ daha fazla anlama.

---

## 7. Niyet birinci sınıf kavramdır.

Event'ler ham maddedir.
Niyet üretilmiş bilgidir.

- **BDD:** event → assertion
- **Intentum:** event → sinyal → anlam → güven

Bu zincir koparılamaz.

---

## 8. Özet cümle

> **Loglar ne olduğunu söyler.**  
> **Niyet ne anlama geldiğini söyler.**

---

## Intentum nerede durur?

```
Telemetri → Loglar → Event'ler → Sinyaller → Niyet → Karar
```

Analytics bu zincirin başında kalır.
Intentum zincirin ortasında durur.

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Mimari](architecture.md) veya [Senaryolar](scenarios.md).

Intentum analytics’in alternatifi değil; analytics’in veremediği anlam katmanı.
