const api = (path, options = {}) =>
  fetch(path, { headers: { "Content-Type": "application/json", ...options.headers }, ...options });

function setResult(id, data, isError = false) {
  const el = document.getElementById(id);
  el.textContent = typeof data === "string" ? data : JSON.stringify(data, null, 2);
  el.classList.remove("empty", "ok", "err");
  el.classList.add(isError ? "err" : "ok");
}

document.getElementById("carbon-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const resultId = "carbon-result";
  setResult(resultId, "…", false);
  const estimatedTonsCo2 = form.estimatedTonsCo2.value ? parseFloat(form.estimatedTonsCo2.value) : null;
  try {
    const res = await api("/api/carbon/calculate", {
      method: "POST",
      body: JSON.stringify({
        actor: form.actor.value,
        scope: form.scope.value,
        estimatedTonsCo2,
      }),
    });
    const data = await res.json();
    if (!res.ok) setResult(resultId, data, true);
    else setResult(resultId, data, false);
  } catch (err) {
    setResult(resultId, err.message, true);
  }
});

document.getElementById("report-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const reportId = e.target.reportId.value;
  const resultId = "report-result";
  setResult(resultId, "…", false);
  try {
    const res = await api(`/api/carbon/report/${encodeURIComponent(reportId)}`);
    const data = await res.json();
    if (res.status === 404) setResult(resultId, "Report not found", true);
    else setResult(resultId, data, false);
  } catch (err) {
    setResult(resultId, err.message, true);
  }
});

document.getElementById("order-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const resultId = "order-result";
  setResult(resultId, "…", false);
  try {
    const res = await api("/api/orders", {
      method: "POST",
      body: JSON.stringify({
        productId: form.productId.value,
        quantity: parseInt(form.quantity.value, 10),
        customerId: form.customerId.value,
      }),
    });
    const data = await res.json();
    if (!res.ok) setResult(resultId, data, true);
    else setResult(resultId, data, false);
  } catch (err) {
    setResult(resultId, err.message, true);
  }
});

document.querySelectorAll(".result").forEach((el) => el.classList.add("empty"));
document.querySelectorAll(".result").forEach((el) => (el.textContent = "Submit a form to see result."));
