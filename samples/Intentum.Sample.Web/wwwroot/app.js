/* --- API helper (POST için cache yok, sonuç her seferinde güncellenir) --- */
const api = (path, options = {}) => {
  const opts = { ...options, headers: { "Content-Type": "application/json", ...options.headers } };
  if (options.method === "POST" || (options.method && options.method.toUpperCase() === "POST"))
    opts.cache = "no-store";
  return fetch(path, opts);
};

/* --- Result display (with fade-in and loading) --- */
function setResult(id, data, isError = false) {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = typeof data === "string" ? data : JSON.stringify(data, null, 2);
  el.classList.remove("empty", "ok", "err");
  el.classList.add("ok", "fade-in");
  if (isError) el.classList.remove("ok"), el.classList.add("err");
  else el.classList.remove("empty");
}

function setResultLoading(id, message = "Yükleniyor…") {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = message;
  el.classList.remove("empty", "ok", "err");
  el.classList.add("empty");
}

/* --- Top tabs: Örnekler | Playground | Dashboard --- */
const viewExamples = document.getElementById("view-examples");
const viewPlayground = document.getElementById("view-playground");
const viewDashboard = document.getElementById("view-dashboard");
const viewHelp = document.getElementById("view-help");
const tabExamples = document.getElementById("tab-examples");
const tabPlayground = document.getElementById("tab-playground");
const tabDashboard = document.getElementById("tab-dashboard");
const tabHelp = document.getElementById("tab-help");

let liveRefreshInterval = null;
const LIVE_REFRESH_MS = 15000;

function showView(name) {
  const isExamples = name === "examples";
  const isPlayground = name === "playground";
  const isDashboard = name === "dashboard";
  const isHelp = name === "help";
  viewExamples?.setAttribute("aria-hidden", !isExamples);
  viewExamples && (viewExamples.hidden = !isExamples);
  viewPlayground?.setAttribute("aria-hidden", !isPlayground);
  viewPlayground && (viewPlayground.hidden = !isPlayground);
  viewDashboard?.setAttribute("aria-hidden", !isDashboard);
  viewDashboard && (viewDashboard.hidden = !isDashboard);
  viewHelp?.setAttribute("aria-hidden", !isHelp);
  viewHelp && (viewHelp.hidden = !isHelp);
  tabExamples?.setAttribute("aria-selected", isExamples);
  tabExamples?.classList.toggle("active", isExamples);
  tabPlayground?.setAttribute("aria-selected", isPlayground);
  tabPlayground?.classList.toggle("active", isPlayground);
  tabDashboard?.setAttribute("aria-selected", isDashboard);
  tabDashboard?.classList.toggle("active", isDashboard);
  tabHelp?.setAttribute("aria-selected", isHelp);
  tabHelp?.classList.toggle("active", isHelp);
  if (isDashboard) {
    setMonitoringDates(defaultFrom(), defaultTo());
    loadMonitoringSummary();
    loadHistory();
    loadGreenwashingRecent();
    updateExportLinks();
    const liveCheck = document.getElementById("mon-live");
    if (liveCheck?.checked) startLiveRefresh();
    startGreenwashingRecentRefresh();
  } else {
    stopLiveRefresh();
    stopGreenwashingRecentRefresh();
  }
}

function startLiveRefresh() {
  stopLiveRefresh();
  liveRefreshInterval = setInterval(() => {
    loadMonitoringSummary();
    loadHistory();
    loadGreenwashingRecent();
    updateExportLinks();
  }, LIVE_REFRESH_MS);
}

function stopLiveRefresh() {
  if (liveRefreshInterval) {
    clearInterval(liveRefreshInterval);
    liveRefreshInterval = null;
  }
}

tabExamples?.addEventListener("click", () => showView("examples"));
tabPlayground?.addEventListener("click", () => showView("playground"));
tabDashboard?.addEventListener("click", () => showView("dashboard"));
tabHelp?.addEventListener("click", () => showView("help"));

document.getElementById("mon-live")?.addEventListener("change", (e) => {
  if (e.target.checked) startLiveRefresh();
  else stopLiveRefresh();
});

/* --- Sidebar: smooth scroll to section (sadece Örnekler görünümünde) --- */
document.querySelectorAll(".sidebar-link").forEach((link) => {
  link.addEventListener("click", (e) => {
    const href = link.getAttribute("href");
    if (href?.startsWith("#")) {
      e.preventDefault();
      const target = document.querySelector(href);
      target?.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  });
});

/* --- Events list (add/remove rows) for Infer and Explain --- */
function escapeAttr(s) {
  const t = String(s == null ? "" : s);
  return t.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

function createEventRow(actor = "user", action = "login") {
  const li = document.createElement("li");
  li.innerHTML = `
    <label>Aktör <input type="text" name="actor" value="${escapeAttr(actor)}" /></label>
    <label>Aksiyon <input type="text" name="action" value="${escapeAttr(action)}" /></label>
    <button type="button" class="btn-remove" aria-label="Olayı kaldır">−</button>
  `;
  li.querySelector(".btn-remove").addEventListener("click", () => li.remove());
  return li;
}

function setEventsList(listId, events) {
  const list = document.getElementById(listId);
  if (!list) return;
  list.innerHTML = "";
  (events || []).forEach((e) => list.appendChild(createEventRow(e.actor, e.action)));
}

function getEventsFromList(listEl) {
  const items = listEl.querySelectorAll("li");
  return Array.from(items).map((li) => {
    const actor = li.querySelector('input[name="actor"]')?.value ?? "";
    const action = li.querySelector('input[name="action"]')?.value ?? "";
    return { actor, action };
  }).filter((e) => e.actor.trim() || e.action.trim());
}

function initEventsList(listId, addButtonId, defaultEvents = [{ actor: "user", action: "login" }, { actor: "user", action: "submit" }]) {
  const list = document.getElementById(listId);
  const addBtn = document.getElementById(addButtonId);
  if (!list || !addBtn) return;

  defaultEvents.forEach((e) => list.appendChild(createEventRow(e.actor, e.action)));
  addBtn.addEventListener("click", () => list.appendChild(createEventRow("user", "action")));
}

initEventsList("infer-events-list", "infer-add-event");
initEventsList("explain-events-list", "explain-add-event", [{ actor: "user", action: "login" }]);
initEventsList("playground-events-list", "playground-add-event", [
  { actor: "user", action: "login" },
  { actor: "user", action: "submit" },
]);

/* Preset scenarios: fill infer events, explain list, and optional narrative */
document.querySelectorAll(".btn-preset").forEach((btn) => {
  btn.addEventListener("click", () => {
    try {
      const events = JSON.parse(btn.getAttribute("data-events") || "[]");
      setEventsList("infer-events-list", events);
      setEventsList("explain-events-list", events);
      const narrativeEl = document.getElementById("infer-narrative");
      const narrative = btn.getAttribute("data-narrative");
      if (narrativeEl) {
        narrativeEl.textContent = narrative || "";
        narrativeEl.style.display = narrative ? "block" : "none";
      }
    } catch (_) {}
  });
});

/* Playground presets: fill list and clear JSON so user can edit */
document.querySelectorAll(".btn-preset-pg").forEach((btn) => {
  btn.addEventListener("click", () => {
    try {
      const events = JSON.parse(btn.getAttribute("data-events") || "[]");
      setEventsList("playground-events-list", events);
      const jsonEl = document.getElementById("playground-json");
      if (jsonEl) jsonEl.value = "";
    } catch (_) {}
  });
});

/* Playground: JSON'dan listeye uygula */
document.getElementById("playground-json-apply")?.addEventListener("click", () => {
  const jsonEl = document.getElementById("playground-json");
  const raw = jsonEl?.value?.trim();
  if (!raw) return;
  try {
    const arr = JSON.parse(raw);
    if (!Array.isArray(arr)) return;
    const events = arr.map((e) => ({ actor: String(e?.actor ?? ""), action: String(e?.action ?? "") }));
    setEventsList("playground-events-list", events.length ? events : [{ actor: "user", action: "login" }]);
  } catch (err) {
    const resultEl = document.getElementById("playground-result");
    if (resultEl) resultEl.innerHTML = `<p class="result err">Geçersiz JSON: ${escapeHtml(err.message)}</p>`;
  }
});

/* Playground: Listeden JSON'a aktar */
document.getElementById("playground-json-from-list")?.addEventListener("click", () => {
  const listEl = document.getElementById("playground-events-list");
  const jsonEl = document.getElementById("playground-json");
  if (!listEl || !jsonEl) return;
  const events = getEventsFromList(listEl);
  jsonEl.value = events.length ? JSON.stringify(events, null, 2) : "[]";
});

/* --- Intent Infer --- */
const inferForm = document.getElementById("infer-form");

function decisionBadgeClass(decision) {
  const d = (decision || "").toLowerCase().replace(/\s/g, "_");
  if (["allow", "block", "observe", "warn", "escalate", "requireauth", "ratelimit"].includes(d))
    return `badge-${d === "requireauth" ? "require_auth" : d === "ratelimit" ? "rate_limit" : d}`;
  return "badge-observe";
}

function getInferResultEl() {
  return document.getElementById("infer-result");
}

inferForm?.addEventListener("submit", async (e) => {
  e.preventDefault();
  const resultEl = getInferResultEl();
  const listEl = document.getElementById("infer-events-list");
  const events = listEl ? getEventsFromList(listEl) : [];
  if (events.length === 0) {
    if (resultEl) {
      resultEl.innerHTML = '<p class="result err">En az bir olay ekleyin (aktör + aksiyon).</p>';
      resultEl.classList.add("fade-in");
    }
    return;
  }
  const btn = document.getElementById("infer-submit");
  if (btn) {
    btn.disabled = true;
    btn.textContent = "Çıkarılıyor…";
  }
  if (resultEl) {
    resultEl.innerHTML = '<p class="result empty">Çıkarılıyor…</p>';
    resultEl.classList.add("fade-in");
  }
  try {
    const res = await api("/api/intent/infer", { method: "POST", body: JSON.stringify({ events }) });
    const data = await res.json();
    const el = getInferResultEl();
    if (!el) return;
    if (!res.ok) {
      el.innerHTML = `<pre class="result err fade-in">${JSON.stringify(data, null, 2)}</pre>`;
      return;
    }
    el.innerHTML = `
      <div class="result ok fade-in">
        <p><strong>Karar</strong> <span class="badge ${decisionBadgeClass(data.decision)}">${escapeHtml(data.decision)}</span></p>
        <p><strong>Güven</strong> ${escapeHtml(data.confidence)}</p>
        <p><strong>Rate limit</strong> ${data.rateLimitAllowed ? "OK" : "Sınırlı"} ${data.rateLimitCurrent != null ? `(${data.rateLimitCurrent}/${data.rateLimitLimit})` : ""}</p>
        <p><strong>Geçmiş ID</strong> <code>${escapeHtml(data.historyId)}</code></p>
      </div>
    `;
  } catch (err) {
    const el = getInferResultEl();
    if (el) el.innerHTML = `<pre class="result err fade-in">${escapeHtml(err.message)}</pre>`;
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = "Çıkar";
    }
  }
});

/* --- Intent Explain --- */
const explainForm = document.getElementById("explain-form");

function getExplainResultEl() {
  return document.getElementById("explain-result");
}

explainForm?.addEventListener("submit", async (e) => {
  e.preventDefault();
  const resultEl = getExplainResultEl();
  const listEl = document.getElementById("explain-events-list");
  const events = listEl ? getEventsFromList(listEl) : [];
  if (events.length === 0) {
    if (resultEl) {
      resultEl.innerHTML = '<p class="result err">En az bir olay ekleyin.</p>';
      resultEl.classList.add("fade-in");
    }
    return;
  }
  const btn = document.getElementById("explain-submit");
  if (btn) {
    btn.disabled = true;
    btn.textContent = "Açıklanıyor…";
  }
  if (resultEl) {
    resultEl.innerHTML = '<p class="result empty">Açıklanıyor…</p>';
    resultEl.classList.add("fade-in");
  }
  try {
    const res = await api("/api/intent/explain", { method: "POST", body: JSON.stringify({ events }) });
    const data = await res.json();
    const el = getExplainResultEl();
    if (!el) return;
    if (!res.ok) {
      el.innerHTML = `<pre class="result err fade-in">${JSON.stringify(data, null, 2)}</pre>`;
      return;
    }
    const rows = (data.signalContributions || []).map(
      (c) => `<tr><td>${escapeHtml(c.source)}</td><td>${escapeHtml(c.description)}</td><td>${c.weight}</td><td>${c.contributionPercent != null ? c.contributionPercent.toFixed(1) + "%" : "-"}</td></tr>`
    ).join("");
    el.innerHTML = `
      <div class="fade-in">
        <p><strong>Niyet</strong> ${escapeHtml(data.intentName)} · <strong>Güven</strong> ${escapeHtml(data.confidence)}</p>
        <p class="subtitle">${escapeHtml(data.explanation || "")}</p>
        <div class="table-wrap">
          <table class="signals">
            <thead><tr><th>Kaynak</th><th>Açıklama</th><th>Ağırlık</th><th>Katkı %</th></tr></thead>
            <tbody>${rows || "<tr><td colspan='4'>Sinyal yok</td></tr>"}</tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    const el = getExplainResultEl();
    if (el) el.innerHTML = `<pre class="result err fade-in">${escapeHtml(err.message)}</pre>`;
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = "Açıkla";
    }
  }
});

function escapeHtml(s) {
  const div = document.createElement("div");
  div.textContent = s == null ? "" : String(s);
  return div.innerHTML;
}

/* --- Playground: compare models --- */
const playgroundForm = document.getElementById("playground-form");

playgroundForm?.addEventListener("submit", async (e) => {
  e.preventDefault();
  let events = [];
  const jsonEl = document.getElementById("playground-json");
  const jsonRaw = jsonEl?.value?.trim();
  if (jsonRaw) {
    try {
      const arr = JSON.parse(jsonRaw);
      if (Array.isArray(arr)) events = arr.map((o) => ({ actor: String(o?.actor ?? ""), action: String(o?.action ?? "") })).filter((e) => e.actor || e.action);
    } catch (_) {}
  }
  if (events.length === 0) {
    const listEl = document.getElementById("playground-events-list");
    events = listEl ? getEventsFromList(listEl) : [];
  }
  const providerCheckboxes = document.querySelectorAll('#playground-form input[name="provider"]:checked');
  const providers = Array.from(providerCheckboxes).map((cb) => cb.value).filter(Boolean);
  const resultEl = document.getElementById("playground-result");
  const btn = document.getElementById("playground-submit");
  if (events.length === 0) {
    if (resultEl) {
      resultEl.innerHTML = '<p class="result err">En az bir olay ekleyin (aktör + aksiyon).</p>';
      resultEl.classList.add("fade-in");
    }
    return;
  }
  if (providers.length === 0) {
    if (resultEl) {
      resultEl.innerHTML = '<p class="result err">En az bir model seçin (Default veya Mock).</p>';
      resultEl.classList.add("fade-in");
    }
    return;
  }
  if (btn) {
    btn.disabled = true;
    btn.textContent = "Karşılaştırılıyor…";
  }
  if (resultEl) {
    resultEl.innerHTML = '<p class="result empty">Karşılaştırılıyor…</p>';
    resultEl.classList.add("fade-in");
  }
  try {
    const res = await api("/api/intent/playground/compare", {
      method: "POST",
      body: JSON.stringify({ events, providers }),
    });
    const data = await res.json();
    const el = document.getElementById("playground-result");
    if (!el) return;
    if (!res.ok) {
      el.innerHTML = `<pre class="result err fade-in">${escapeHtml(JSON.stringify(data, null, 2))}</pre>`;
      return;
    }
    const results = data.results || [];
    if (results.length === 0) {
      el.innerHTML = '<p class="result empty fade-in">Seçilen modellerden sonuç dönmedi.</p>';
      return;
    }
    const rows = results.map(
      (r) => `
      <tr>
        <td><strong>${escapeHtml(r.provider)}</strong></td>
        <td>${escapeHtml(r.intentName)}</td>
        <td>${escapeHtml(r.confidenceLevel)}</td>
        <td>${(r.confidenceScore ?? 0).toFixed(2)}</td>
        <td><span class="badge ${decisionBadgeClass(r.decision)}">${escapeHtml(r.decision)}</span></td>
      </tr>
    `
    ).join("");
    el.innerHTML = `
      <div class="fade-in">
        <div class="table-wrap">
          <table class="playground-table">
            <thead><tr><th>Model</th><th>Niyet</th><th>Güven</th><th>Skor</th><th>Karar</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>
        </div>
        <p class="muted" style="margin-top:0.75rem">Aynı olay seti üzerinde model başına intent ve politika kararı.</p>
      </div>
    `;
  } catch (err) {
    const el = document.getElementById("playground-result");
    if (el) el.innerHTML = `<pre class="result err fade-in">${escapeHtml(err.message)}</pre>`;
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = "Karşılaştır";
    }
  }
});

/* --- Greenwashing: preset reports and analyze --- */
const GREENWASHING_PRESETS = {
  genuine:
    "Sürdürülebilirlik raporumuz ISO 14001 ve ISO 50001 sertifikalarına dayanmaktadır. " +
    "Üçüncü taraf denetim (Deloitte, 2024) sonuçları: tesis A için 42 ton CO₂ azalımı, doğrulanmış veri. " +
    "Su kullanımı 2022 baz yılına göre %18 azaltıldı; metodoloji GRI 303 uyumludur. " +
    "Tedarik zinciri karbon ayak izi hesaplaması Science Based Targets kriterlerine göre raporlanmaktadır.",
  greenwashing:
    "EcoCorp sürdürülebilir bir gelecek ve yeşil dönüşüme kendini adamıştır. Değerlerimiz doğaya saygı ve ekolojik dengeyi içerir. " +
    "%40 emisyon azalımı ve %25 daha az su kullanımı elde ettik. Temiz üretim yöntemlerimiz çevreyi desteklemektedir. " +
    "Karbon nötrlüğe doğru bir yolculuktayız. Detaylar için yıllık raporumuza bakın.",
  borderline:
    "Şirketimiz çevresel performansını iyileştirmektedir. Son üç yılda enerji tüketimimiz azaldı; " +
    "bazı tesislerde ISO 14001 denetimi yapılmaktadır. Sürdürülebilir gelecek hedefimizle daha yeşil ve daha verimli olmaya çalışıyoruz. " +
    "Su ve atık verileri yıllık raporumuzda yer almaktadır; karşılaştırma bazı yıllara göre yapılmaktadır.",
  vague_only:
    "Sürdürülebilir gelecek ve yeşil dönüşüm. Eko dostu, temiz üretim, ekolojik denge. " +
    "Doğaya saygı, karbon nötr hedefi. Çevre dostu değerler. Yeşil yatırımlar.",
  social:
    "[Sosyal medya – mock] EcoCorp artık %100 yeşil enerji kullanıyor! 🌱 Sürdürülebilir gelecek için buradayız. " +
    "Doğaya saygı, ekolojik denge. Detaylar web sitemizde. #sürdürülebilirlik #yeşil",
  press:
    "[Basın bülteni – mock] Şirketimiz sürdürülebilir bir geleceğe yatırım yapıyor. " +
    "%30 emisyon azalımı hedefleniyor. Temiz üretim ve yeşil dönüşüm önceliğimiz. " +
    "Karbon nötrlüğe doğru adımlar atıyoruz. Yatırımcı bilgilendirmesi için yıllık rapora bakınız.",
  investor:
    "[Yatırımcı sunumu – mock] Sustainable future and green transition. " +
    "40% emissions reduction, 25% less water. Clean production, ecological balance. " +
    "Path to carbon neutrality. Third-party verification in progress. Scope 3 roadmap 2026.",
};

document.querySelectorAll(".btn-preset-green").forEach((btn) => {
  btn.addEventListener("click", () => {
    const preset = btn.getAttribute("data-preset");
    const ta = document.getElementById("greenwashing-report");
    const sourceInput = document.getElementById("greenwashing-source-type");
    if (ta && GREENWASHING_PRESETS[preset]) ta.value = GREENWASHING_PRESETS[preset];
    if (sourceInput) sourceInput.value = btn.getAttribute("data-source") || "";
    document.querySelectorAll(".btn-preset-source").forEach((b) => b.classList.remove("active"));
  });
});

document.querySelectorAll(".btn-preset-source").forEach((btn) => {
  btn.addEventListener("click", () => {
    const source = btn.getAttribute("data-source") || "";
    const preset = btn.getAttribute("data-preset") || "";
    const ta = document.getElementById("greenwashing-report");
    const sourceInput = document.getElementById("greenwashing-source-type");
    if (sourceInput) sourceInput.value = source;
    if (ta && preset && GREENWASHING_PRESETS[preset]) ta.value = GREENWASHING_PRESETS[preset];
    document.querySelectorAll(".btn-preset-source").forEach((b) => b.classList.toggle("active", b === btn));
  });
});

const greenwashingForm = document.getElementById("greenwashing-form");
function getGreenwashingResultEl() {
  return document.getElementById("greenwashing-result");
}

greenwashingForm?.addEventListener("submit", async (e) => {
  e.preventDefault();
  const report = document.getElementById("greenwashing-report")?.value?.trim() ?? "";
  const resultEl = getGreenwashingResultEl();
  const btn = document.getElementById("greenwashing-submit");
  if (!report) {
    if (resultEl) {
      resultEl.innerHTML = '<p class="result err">Rapor metni girin veya hazır rapor seçin.</p>';
      resultEl.classList.add("fade-in");
    }
    return;
  }
  if (btn) {
    btn.disabled = true;
    btn.textContent = "Analiz ediliyor…";
  }
  if (resultEl) {
    resultEl.innerHTML = '<p class="result empty">Analiz ediliyor…</p>';
    resultEl.classList.add("fade-in");
  }
  const sourceType = document.getElementById("greenwashing-source-type")?.value || "";
  const language = document.getElementById("greenwashing-language")?.value || "";
  const fileInput = document.getElementById("greenwashing-image");
  let imageBase64 = "";
  if (fileInput?.files?.[0]) {
    const file = fileInput.files[0];
    const buf = await file.arrayBuffer();
    const bytes = new Uint8Array(buf);
    let binary = "";
    for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i]);
    imageBase64 = btoa(binary);
  }
  try {
    const res = await api("/api/greenwashing/analyze", {
      method: "POST",
      body: JSON.stringify({
        report,
        sourceType: sourceType || undefined,
        language: language || undefined,
        imageBase64: imageBase64 || undefined,
      }),
    });
    const data = await res.json();
    const el = getGreenwashingResultEl();
    if (!el) return;
    if (!res.ok) {
      el.innerHTML = `<pre class="result err fade-in">${JSON.stringify(data, null, 2)}</pre>`;
      return;
    }
    const signalsList = (data.signalDescriptions || []).length
      ? `<ul class="signal-list">${(data.signalDescriptions || []).map((s) => `<li>${escapeHtml(s)}</li>`).join("")}</ul>`
      : "<p class=\"muted\">Sinyal yok</p>";
    const actionsList = (data.suggestedActions || []).length
      ? `<ul class="action-list">${(data.suggestedActions || []).map((a) => `<li>${escapeHtml(a)}</li>`).join("")}</ul>`
      : "<p class=\"muted\">Öneri yok</p>";
    const meta = data.sourceMetadata;
    const scope3Block = meta?.scope3Summary
      ? `<p><strong>Scope 3 (mock)</strong> ${meta.scope3Summary.verifiedCount}/${meta.scope3Summary.totalSuppliers} tedarikçi doğrulandı</p><ul class="signal-list">${(meta.scope3Summary.details || []).map((d) => `<li>${escapeHtml(d.name)} ${d.verified ? "✓" : "—"}</li>`).join("")}</ul>`
      : meta
        ? `<p><strong>Scope 3 doğrulaması</strong> ${meta.scope3Verified ? "Evet" : "Hayır"}</p>`
        : "";
    const metaBlock = meta
      ? `<div class="source-metadata"><h4 class="result-subtitle">Kaynak (mock)</h4><p><strong>Kaynak</strong> ${escapeHtml(meta.sourceType)} · <strong>Dil</strong> ${escapeHtml(meta.language || "—")} · <strong>Blockchain</strong> <code>${escapeHtml(data.blockchainRef || meta.blockchainRef || "—")}</code></p>${scope3Block}<p class="muted">Analiz: ${new Date(meta.analyzedAt).toLocaleString("tr-TR")}</p></div>`
      : data.blockchainRef
        ? `<div class="source-metadata"><p><strong>Kayıt ref (Blockchain demo)</strong> <code>${escapeHtml(data.blockchainRef)}</code></p></div>`
        : "";
    const visualBlock = data.visualResult
      ? `<div class="source-metadata"><h4 class="result-subtitle">Görsel (demo)</h4><p><strong>Yeşillik skoru</strong> ${(data.visualResult.greenScore * 100).toFixed(0)}% · ${escapeHtml(data.visualResult.label)}</p></div>`
      : "";
    el.innerHTML = `
      <div class="result ok fade-in">
        <p><strong>Niyet</strong> ${escapeHtml(data.intentName)}</p>
        <p><strong>Güven</strong> ${escapeHtml(data.confidence)} (skor: ${(data.confidenceScore ?? 0).toFixed(2)})</p>
        <p><strong>Karar</strong> <span class="badge ${decisionBadgeClass(data.decision)}">${escapeHtml(data.decision)}</span></p>
        ${metaBlock}
        ${visualBlock}
        <h4 class="result-subtitle">Sinyaller</h4>
        ${signalsList}
        <h4 class="result-subtitle">Önerilen aksiyonlar</h4>
        ${actionsList}
      </div>
    `;
    loadGreenwashingRecent();
  } catch (err) {
    const el = getGreenwashingResultEl();
    if (el) el.innerHTML = `<pre class="result err fade-in">${escapeHtml(err.message)}</pre>`;
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = "Analiz et";
    }
  }
});

/* --- Carbon, Report, Order forms --- */
document.getElementById("carbon-form")?.addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const resultId = "carbon-result";
  setResultLoading(resultId, "…");
  const estimatedTonsCo2 = form.estimatedTonsCo2.value ? Number.parseFloat(form.estimatedTonsCo2.value) : null;
  try {
    const res = await api("/api/carbon/calculate", {
      method: "POST",
      body: JSON.stringify({ actor: form.actor.value, scope: form.scope.value, estimatedTonsCo2 }),
    });
    const data = await res.json();
    setResult(resultId, data, !res.ok);
    document.getElementById(resultId).classList.add("fade-in");
  } catch (err) {
    setResult(resultId, err.message, true);
    document.getElementById(resultId).classList.add("fade-in");
  }
});

document.getElementById("report-form")?.addEventListener("submit", async (e) => {
  e.preventDefault();
  const reportId = e.target.reportId.value;
  const resultId = "report-result";
  setResultLoading(resultId, "…");
  try {
    const res = await api(`/api/carbon/report/${encodeURIComponent(reportId)}`);
    const data = await res.json();
    if (res.status === 404) setResult(resultId, "Rapor bulunamadı", true);
    else setResult(resultId, data, false);
    document.getElementById(resultId).classList.add("fade-in");
  } catch (err) {
    setResult(resultId, err.message, true);
    document.getElementById(resultId).classList.add("fade-in");
  }
});

document.getElementById("order-form")?.addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const resultId = "order-result";
  setResultLoading(resultId, "…");
  try {
    const res = await api("/api/orders", {
      method: "POST",
      body: JSON.stringify({
        productId: form.productId.value,
        quantity: Number.parseInt(form.quantity.value, 10),
        customerId: form.customerId.value,
      }),
    });
    const data = await res.json();
    setResult(resultId, data, !res.ok);
    document.getElementById(resultId).classList.add("fade-in");
  } catch (err) {
    setResult(resultId, err.message, true);
    document.getElementById(resultId).classList.add("fade-in");
  }
});

/* --- Monitoring: date range helpers --- */
function toLocalISO(d) {
  const pad = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00`;
}

function defaultFrom() {
  const d = new Date();
  d.setDate(d.getDate() - 7);
  return d;
}

function defaultTo() {
  return new Date();
}

function setMonitoringDates(from, to) {
  const fromEl = document.getElementById("mon-from");
  const toEl = document.getElementById("mon-to");
  if (fromEl) fromEl.value = toLocalISO(from);
  if (toEl) toEl.value = toLocalISO(to);
}

function getMonitoringDates() {
  const fromEl = document.getElementById("mon-from");
  const toEl = document.getElementById("mon-to");
  const from = fromEl?.value ? new Date(fromEl.value) : defaultFrom();
  const to = toEl?.value ? new Date(toEl.value) : defaultTo();
  return { from, to };
}

/* --- Monitoring: fetch summary and render --- */
const decisionNames = ["Allow", "Observe", "Warn", "Block", "Escalate", "RequireAuth", "RateLimit"];
const decisionColors = {
  Allow: "var(--decision-allow)",
  Block: "var(--decision-block)",
  Observe: "var(--decision-observe)",
  Warn: "var(--decision-warn)",
  Escalate: "var(--decision-escalate)",
  RequireAuth: "var(--decision-require-auth)",
  RateLimit: "var(--decision-rate-limit)",
};

function decisionLabel(d) {
  if (typeof d === "number" && d >= 0 && d < decisionNames.length) return decisionNames[d];
  return String(d ?? "");
}

function animateValue(el, end, duration = 600) {
  if (typeof el === "string") el = document.querySelector(el);
  if (!el || typeof end !== "number") return;
  if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    el.textContent = end;
    return;
  }
  const start = 0;
  const startTime = performance.now();
  function step(now) {
    const t = Math.min((now - startTime) / duration, 1);
    const v = Math.round(start + (end - start) * t);
    el.textContent = v;
    if (t < 1) requestAnimationFrame(step);
  }
  requestAnimationFrame(step);
}

function renderMonitoringSummary(summary) {
  const kpiEl = document.getElementById("mon-kpi");
  const barsEl = document.getElementById("mon-decision-bars");
  const trendTbody = document.querySelector("#mon-trend-table tbody");
  const anomaliesEl = document.getElementById("mon-anomalies");

  if (kpiEl) {
    kpiEl.innerHTML = `
      <div class="kpi-card"><div class="value" data-count="${summary.totalInferences}">0</div><div class="label">Toplam çıkarım</div></div>
      <div class="kpi-card"><div class="value" data-count="${summary.uniqueBehaviorSpaces}">0</div><div class="label">Benzersiz davranış alanı</div></div>
    `;
    kpiEl.querySelectorAll(".value[data-count]").forEach((el) => {
      const end = parseInt(el.getAttribute("data-count"), 10);
      if (!isNaN(end)) animateValue(el, end);
      else el.textContent = "0";
    });
  }
  updateExportLinks();

  if (barsEl && summary.decisionDistribution) {
    const dist = summary.decisionDistribution.countByDecision || {};
    const total = summary.decisionDistribution.totalCount || 1;
    barsEl.innerHTML = decisionNames.map((name, idx) => {
      const count = dist[name] ?? dist[String(idx)] ?? dist[idx] ?? 0;
      const pct = total ? Math.round((count / total) * 100) : 0;
      const color = decisionColors[name] || "var(--border)";
      return `
        <div class="decision-bar-row">
          <span class="name">${escapeHtml(name)}</span>
          <div class="bar-wrap"><div class="bar" style="width:${pct}%;background:${color}"></div></div>
          <span class="muted" style="font-size:0.85rem;color:var(--text-muted)">${count}</span>
        </div>
      `;
    }).join("");
  }

  if (trendTbody && summary.confidenceTrend && summary.confidenceTrend.length) {
    trendTbody.innerHTML = summary.confidenceTrend.slice(0, 20).map((p) => `
      <tr>
        <td>${new Date(p.bucketStart).toLocaleString("tr-TR")}</td>
        <td>${escapeHtml(p.confidenceLevel)}</td>
        <td>${p.count}</td>
        <td>${(p.averageScore ?? 0).toFixed(2)}</td>
      </tr>
    `).join("");
  } else if (trendTbody) {
    trendTbody.innerHTML = "<tr><td colspan='4'>Eğilim verisi yok</td></tr>";
  }

  if (anomaliesEl && summary.anomalies && summary.anomalies.length) {
    anomaliesEl.innerHTML = summary.anomalies.map((a) => `
      <li>
        <div class="type">${escapeHtml(a.type)}</div>
        <div class="desc">${escapeHtml(a.description)}</div>
        <div class="meta">${new Date(a.detectedAt).toLocaleString("tr-TR")} · Şiddet ${a.severity}</div>
      </li>
    `).join("");
  } else if (anomaliesEl) {
    anomaliesEl.innerHTML = "<li><span class='desc'>Anomali tespit edilmedi</span></li>";
  }
}

async function loadMonitoringSummary() {
  const { from, to } = getMonitoringDates();
  const fromStr = from.toISOString();
  const toStr = to.toISOString();
  const loadingEl = document.getElementById("mon-loading");
  const contentEl = document.getElementById("mon-content");
  if (loadingEl) loadingEl.style.display = "block";
  if (contentEl) contentEl.style.display = "none";
  try {
    const res = await api(`/api/intent/analytics/summary?from=${encodeURIComponent(fromStr)}&to=${encodeURIComponent(toStr)}`);
    const summary = await res.json();
    if (loadingEl) loadingEl.style.display = "none";
    if (contentEl) contentEl.style.display = "block";
    renderMonitoringSummary(summary);
  } catch (err) {
    if (loadingEl) loadingEl.style.display = "none";
    if (contentEl) {
      contentEl.innerHTML = `<p class="result err">${escapeHtml(err.message)}</p>`;
      contentEl.style.display = "block";
    }
  }
}

async function loadHistory() {
  const { from, to } = getMonitoringDates();
  const fromStr = from.toISOString();
  const toStr = to.toISOString();
  const loadingEl = document.getElementById("history-loading");
  const wrapEl = document.getElementById("history-wrap");
  const tbody = document.querySelector("#history-table tbody");
  if (!tbody) return;
  if (loadingEl) loadingEl.style.display = "block";
  if (wrapEl) wrapEl.style.display = "none";
  try {
    const res = await api(`/api/intent/history?from=${encodeURIComponent(fromStr)}&to=${encodeURIComponent(toStr)}&limit=50`);
    const records = await res.json();
    if (loadingEl) loadingEl.style.display = "none";
    if (wrapEl) wrapEl.style.display = "block";
    tbody.innerHTML = records.length === 0
      ? "<tr><td colspan='6'>Bu aralıkta kayıt yok</td></tr>"
      : records.map((r) => {
          const dec = decisionLabel(r.decision);
          const meta = r.metadata || {};
          const eventsSummary = meta.eventsSummary ?? meta.EventsSummary ?? meta.source ?? meta.Source ?? "—";
          const summaryText = typeof eventsSummary === "string" ? eventsSummary : (eventsSummary != null ? String(eventsSummary) : "—");
          return `
          <tr>
            <td><code>${escapeHtml(r.id)}</code></td>
            <td>${escapeHtml(r.intentName)}</td>
            <td><span class="events-summary" title="${escapeAttr(summaryText)}">${escapeHtml(summaryText)}</span></td>
            <td>${escapeHtml(r.confidenceLevel)}</td>
            <td><span class="badge ${decisionBadgeClass(dec)}">${escapeHtml(dec)}</span></td>
            <td>${new Date(r.recordedAt).toLocaleString("tr-TR")}</td>
          </tr>
        `;
        }).join("");
  } catch (err) {
    if (loadingEl) loadingEl.style.display = "none";
    if (wrapEl) {
      wrapEl.style.display = "block";
      tbody.innerHTML = `<tr><td colspan="6" class="err">${escapeHtml(err.message)}</td></tr>`;
    }
  }
}

document.getElementById("mon-refresh")?.addEventListener("click", () => {
  loadMonitoringSummary();
  loadHistory();
});

document.querySelectorAll(".quick-range button").forEach((btn) => {
  btn.addEventListener("click", () => {
    const hours = parseInt(btn.getAttribute("data-range"), 10);
    const to = new Date();
    const from = new Date(to.getTime() - hours * 60 * 60 * 1000);
    setMonitoringDates(from, to);
    loadMonitoringSummary();
    loadHistory();
    updateExportLinks();
  });
});

/* Export links: set href with current from/to */
function updateExportLinks() {
  const { from, to } = getMonitoringDates();
  const fromStr = encodeURIComponent(from.toISOString());
  const toStr = encodeURIComponent(to.toISOString());
  const jsonLink = document.getElementById("export-json");
  const csvLink = document.getElementById("export-csv");
  if (jsonLink) jsonLink.href = `/api/intent/analytics/export/json?from=${fromStr}&to=${toStr}`;
  if (csvLink) csvLink.href = `/api/intent/analytics/export/csv?from=${fromStr}&to=${toStr}`;
}

document.getElementById("export-json")?.addEventListener("click", () => updateExportLinks());
document.getElementById("export-csv")?.addEventListener("click", () => updateExportLinks());

/* --- Greenwashing: son analizler (gerçek zamanlı mock) --- */
let greenwashingRecentInterval = null;
const GREENWASHING_RECENT_MS = 20_000;

async function loadGreenwashingRecent() {
  const tbody = document.querySelector("#greenwashing-recent-table tbody");
  if (!tbody) return;
  try {
    const res = await api("/api/greenwashing/recent?limit=15");
    const list = await res.json();
    tbody.innerHTML = list.length === 0
      ? "<tr><td colspan='7'>Henüz analiz yok; analiz yapın veya 30 sn bekleyin (mock kayıt).</td></tr>"
      : list.map((r) => `
          <tr>
            <td><code class="small-ref">${escapeHtml(r.id)}</code></td>
            <td><span class="events-summary" title="${escapeAttr(r.reportPreview)}">${escapeHtml(r.reportPreview)}</span></td>
            <td>${escapeHtml(r.intentName)}</td>
            <td><span class="badge ${decisionBadgeClass(r.decision)}">${escapeHtml(r.decision)}</span></td>
            <td>${escapeHtml(r.sourceType || "—")}</td>
            <td>${escapeHtml(r.language || "—")}</td>
            <td>${new Date(r.analyzedAt).toLocaleString("tr-TR")}</td>
          </tr>
        `).join("");
  } catch (_) {
    tbody.innerHTML = "<tr><td colspan='7'>Yüklenemedi.</td></tr>";
  }
}

function startGreenwashingRecentRefresh() {
  stopGreenwashingRecentRefresh();
  loadGreenwashingRecent();
  greenwashingRecentInterval = setInterval(loadGreenwashingRecent, GREENWASHING_RECENT_MS);
}

function stopGreenwashingRecentRefresh() {
  if (greenwashingRecentInterval) {
    clearInterval(greenwashingRecentInterval);
    greenwashingRecentInterval = null;
  }
}

/* İlk açılış: Örnekler görünür, Dashboard gizli; tarihler dashboard için hazır */
setMonitoringDates(defaultFrom(), defaultTo());

/* Başlangıç: Infer/Explain placeholder */
["infer-result", "explain-result"].forEach((id) => {
  const el = document.getElementById(id);
  if (el && !el.textContent.trim()) el.innerHTML = '<p class="result empty">Sonuç burada görünür.</p>';
});
document.querySelectorAll(".result.empty").forEach((el) => {
  if (el.id && el.id.endsWith("-result") && el.tagName === "PRE") el.textContent = "Sonuç burada görünür.";
});
