import { useEffect, useMemo, useState } from "react";
import api, { apiError } from "../api/client";
import { money } from "../lib/format";

export default function IssueTicket() {
  const [categories, setCategories] = useState([]);
  const [methods, setMethods] = useState([]);
  const [fields, setFields] = useState([]);
  const [settings, setSettings] = useState(null);
  const [available, setAvailable] = useState([]);
  const [form, setForm] = useState({
    ticketCategoryId: "",
    ticketId: "",
    paymentMethodId: "",
    notes: "",
    values: {}
  });
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([
      api.get("/api/ticket-categories"),
      api.get("/api/payment-methods"),
      api.get("/api/custom-fields"),
      api.get("/api/settings")
    ]).then(([cats, pays, custom, sets]) => {
      setCategories(cats.data.filter((c) => c.isActive));
      setMethods(pays.data.filter((m) => m.isActive));
      setFields(custom.data.filter((f) => f.isActive));
      setSettings(sets.data);
    });
  }, []);

  const category = categories.find((c) => String(c.id) === String(form.ticketCategoryId));
  const method = methods.find((m) => String(m.id) === String(form.paymentMethodId));
  const complimentary = Boolean(category?.isVip || category?.isFreeEntry || method?.collectsRevenue === false);
  const blocked = complimentary && settings && !settings.freeEntryEnabled;
  const amount = complimentary ? 0 : category?.price || 0;

  useEffect(() => {
    if (!form.ticketCategoryId) {
      setAvailable([]);
      return;
    }
    api
      .get("/api/tickets", { params: { categoryId: form.ticketCategoryId, status: 0, take: 80 } })
      .then((res) => {
        setAvailable(res.data);
        setForm((prev) => ({ ...prev, ticketId: res.data[0]?.id ? String(res.data[0].id) : "" }));
      });
  }, [form.ticketCategoryId]);

  const selectedTicket = useMemo(
    () => available.find((t) => String(t.id) === String(form.ticketId)),
    [available, form.ticketId]
  );

  function setValue(id, value) {
    setForm((prev) => ({ ...prev, values: { ...prev.values, [id]: value } }));
  }

  async function onSubmit(e) {
    e.preventDefault();
    setError("");
    setMessage("");
    setBusy(true);
    try {
      const payload = {
        ticketCategoryId: Number(form.ticketCategoryId),
        ticketId: form.ticketId ? Number(form.ticketId) : null,
        paymentMethodId: Number(form.paymentMethodId),
        notes: form.notes,
        customerName: form.values[fields.find((f) => f.fieldKey === "customer_name")?.id] || "",
        customerPhone: form.values[fields.find((f) => f.fieldKey === "customer_phone")?.id] || "",
        fieldValues: fields.map((field) => ({
          customFieldId: field.id,
          value: form.values[field.id] || ""
        }))
      };
      const { data } = await api.post("/api/transactions", payload);
      setMessage(`Issued ${data.ticketNumber} to ${data.customerName || "guest"} via ${data.paymentMethodName}.`);
      setForm((prev) => ({ ...prev, notes: "", values: {}, ticketId: "" }));
      const [refreshCats, refresh] = await Promise.all([
        api.get("/api/ticket-categories"),
        api.get("/api/tickets", { params: { categoryId: form.ticketCategoryId, status: 0, take: 80 } })
      ]);
      setCategories(refreshCats.data.filter((c) => c.isActive));
      setAvailable(refresh.data);
      setForm((prev) => ({ ...prev, ticketId: refresh.data[0]?.id ? String(refresh.data[0].id) : "" }));
    } catch (err) {
      setError(apiError(err, "Could not issue ticket."));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div>
      <h1 className="page-title">Issue ticket</h1>
      <p className="page-sub">Gate flow: pick section, unique number, payment method, then customer details.</p>

      <form onSubmit={onSubmit} className="mt-8 grid gap-6 xl:grid-cols-3">
        <section className="glass rounded-3xl p-6 xl:col-span-2">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-400">Ticket section</label>
              <select
                className="field"
                required
                value={form.ticketCategoryId}
                onChange={(e) => setForm((prev) => ({ ...prev, ticketCategoryId: e.target.value }))}
              >
                <option value="">Select category</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name} · {c.remainingTickets} left · {money(c.price, settings?.currency)}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-400">Unique ticket number</label>
              <select
                className="field"
                required
                value={form.ticketId}
                onChange={(e) => setForm((prev) => ({ ...prev, ticketId: e.target.value }))}
              >
                <option value="">Next available</option>
                {available.map((t) => (
                  <option key={t.id} value={t.id}>{t.ticketNumber}</option>
                ))}
              </select>
            </div>
          </div>

          <label className="mb-2 mt-4 block text-xs font-semibold uppercase tracking-wide text-slate-400">Payment method</label>
          <div className="grid gap-3 md:grid-cols-2">
            {methods.map((item) => (
              <button
                type="button"
                key={item.id}
                onClick={() => setForm((prev) => ({ ...prev, paymentMethodId: String(item.id) }))}
                className={`rounded-2xl border p-4 text-left transition ${
                  String(form.paymentMethodId) === String(item.id)
                    ? "border-white/40 bg-white/10"
                    : "border-white/10 bg-ink-950/40 hover:border-white/20"
                }`}
              >
                <div className="mb-2 h-2 w-16 rounded-full" style={{ background: item.color }} />
                <p className="font-semibold">{item.name}</p>
                <p className="text-xs text-slate-400">{item.collectsRevenue ? "Collects revenue" : "Free / no revenue"}</p>
              </button>
            ))}
          </div>

          <div className="mt-6 grid gap-4 md:grid-cols-2">
            {fields.map((field) => (
              <div key={field.id} className={field.fieldType === "textarea" ? "md:col-span-2" : ""}>
                <label className="mb-2 block text-xs font-semibold uppercase tracking-wide text-slate-400">
                  {field.label}{field.isRequired ? " *" : ""}
                </label>
                {field.fieldType === "textarea" ? (
                  <textarea className="field min-h-24" value={form.values[field.id] || ""} onChange={(e) => setValue(field.id, e.target.value)} />
                ) : (
                  <input
                    className="field"
                    type={field.fieldType === "email" ? "email" : field.fieldType === "phone" ? "tel" : field.fieldType === "number" ? "number" : "text"}
                    required={field.isRequired}
                    value={form.values[field.id] || ""}
                    onChange={(e) => setValue(field.id, e.target.value)}
                  />
                )}
              </div>
            ))}
          </div>

          <label className="mb-2 mt-4 block text-xs font-semibold uppercase tracking-wide text-slate-400">Notes</label>
          <textarea className="field min-h-20" value={form.notes} onChange={(e) => setForm((prev) => ({ ...prev, notes: e.target.value }))} />

          {error && <p className="mt-4 rounded-xl bg-rose-500/15 px-3 py-2 text-sm text-rose-300">{error}</p>}
          {message && <p className="mt-4 rounded-xl bg-emerald-500/15 px-3 py-2 text-sm text-emerald-300">{message}</p>}

          <button className="btn-primary mt-6 w-full text-base" disabled={busy || blocked || !form.ticketCategoryId || !form.paymentMethodId}>
            {busy ? "Issuing..." : "Issue ticket now"}
          </button>
        </section>

        <aside className="glass rounded-3xl p-6">
          <p className="text-xs uppercase tracking-wide text-slate-400">Live assignment</p>
          <h2 className="mt-1 text-2xl font-extrabold">{selectedTicket?.ticketNumber || "Select a ticket"}</h2>
          <p className="mt-2 text-slate-400">{category?.name || "No section selected"}</p>
          <div className="mt-6 space-y-3 text-sm">
            <Row label="Price" value={money(amount, settings?.currency)} />
            <Row label="Payment" value={method?.name || "-"} />
            <Row label="Remaining in section" value={category?.remainingTickets ?? "-"} />
            <Row label="VIP / free" value={complimentary ? "Yes — no revenue" : "Paid"} />
          </div>
          {blocked && (
            <p className="mt-6 rounded-xl bg-amber-500/15 p-3 text-sm text-amber-200">
              Free entry is turned off in Settings. Enable it to issue VIP or complimentary tickets.
            </p>
          )}
        </aside>
      </form>
    </div>
  );
}

function Row({ label, value }) {
  return (
    <div className="flex items-center justify-between rounded-xl bg-ink-950/60 px-3 py-2">
      <span className="text-slate-400">{label}</span>
      <span className="font-semibold">{value}</span>
    </div>
  );
}
