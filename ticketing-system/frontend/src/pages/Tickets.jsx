import { useEffect, useState } from "react";
import api, { apiError } from "../api/client";
import { money } from "../lib/format";

const empty = {
  name: "",
  description: "",
  price: 0,
  isVip: false,
  isFreeEntry: false,
  numberPrefix: "",
  startNumber: 1,
  endNumber: 100,
  color: "#22d3ee",
  isActive: true
};

export default function Tickets() {
  const [categories, setCategories] = useState([]);
  const [settings, setSettings] = useState({ currency: "LKR" });
  const [form, setForm] = useState(empty);
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);

  async function load() {
    const [cats, sets] = await Promise.all([api.get("/api/ticket-categories"), api.get("/api/settings")]);
    setCategories(cats.data);
    setSettings(sets.data);
  }

  useEffect(() => {
    load().catch(() => setError("Unable to load ticket categories."));
  }, []);

  function startEdit(category) {
    setEditingId(category.id);
    setForm({
      name: category.name,
      description: category.description,
      price: category.price,
      isVip: category.isVip,
      isFreeEntry: category.isFreeEntry,
      numberPrefix: category.numberPrefix,
      startNumber: category.startNumber,
      endNumber: category.endNumber,
      color: category.color,
      isActive: category.isActive
    });
  }

  async function save(e) {
    e.preventDefault();
    setBusy(true);
    setError("");
    setMessage("");
    try {
      const payload = { ...form, price: Number(form.price), startNumber: Number(form.startNumber), endNumber: Number(form.endNumber) };
      if (editingId) {
        await api.put(`/api/ticket-categories/${editingId}`, payload);
        setMessage("Category updated.");
      } else {
        await api.post("/api/ticket-categories", payload);
        setMessage("Category created. Generate unique numbers next.");
      }
      setForm(empty);
      setEditingId(null);
      await load();
    } catch (err) {
      setError(apiError(err, "Could not save category."));
    } finally {
      setBusy(false);
    }
  }

  async function generate(id) {
    setError("");
    setMessage("");
    try {
      const { data } = await api.post(`/api/ticket-categories/${id}/generate`);
      setMessage(`Generated ${data.created} tickets (${data.skippedExisting} already existed).`);
      await load();
    } catch (err) {
      setError(apiError(err, "Could not generate tickets."));
    }
  }

  return (
    <div>
      <h1 className="page-title">Ticket management</h1>
      <p className="page-sub">Create price / VIP sections and assign unique start–end ticket numbers.</p>

      <div className="mt-8 grid gap-6 xl:grid-cols-5">
        <form onSubmit={save} className="glass rounded-3xl p-6 xl:col-span-2">
          <h2 className="text-lg font-bold">{editingId ? "Edit section" : "New section"}</h2>
          <input className="field mt-4" placeholder="Section name" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          <textarea className="field mt-3 min-h-20" placeholder="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          <div className="mt-3 grid grid-cols-2 gap-3">
            <input className="field" type="number" min="0" placeholder="Price" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
            <input className="field" placeholder="Prefix e.g. GEN-" value={form.numberPrefix} onChange={(e) => setForm({ ...form, numberPrefix: e.target.value })} />
            <input className="field" type="number" placeholder="Start number" required value={form.startNumber} onChange={(e) => setForm({ ...form, startNumber: e.target.value })} />
            <input className="field" type="number" placeholder="End number" required value={form.endNumber} onChange={(e) => setForm({ ...form, endNumber: e.target.value })} />
          </div>
          <div className="mt-3 flex items-center gap-3">
            <input type="color" className="h-11 w-14 rounded-lg border border-white/10 bg-transparent" value={form.color} onChange={(e) => setForm({ ...form, color: e.target.value })} />
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isVip} onChange={(e) => setForm({ ...form, isVip: e.target.checked, isFreeEntry: e.target.checked || form.isFreeEntry, price: e.target.checked ? 0 : form.price })} />
              VIP
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isFreeEntry} onChange={(e) => setForm({ ...form, isFreeEntry: e.target.checked })} />
              Free entry
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
              Active
            </label>
          </div>
          {error && <p className="mt-4 text-sm text-rose-300">{error}</p>}
          {message && <p className="mt-4 text-sm text-emerald-300">{message}</p>}
          <div className="mt-5 flex gap-3">
            <button className="btn-primary flex-1" disabled={busy}>{busy ? "Saving..." : editingId ? "Update section" : "Create section"}</button>
            {editingId && (
              <button type="button" className="btn-ghost" onClick={() => { setEditingId(null); setForm(empty); }}>
                Cancel
              </button>
            )}
          </div>
        </form>

        <div className="space-y-4 xl:col-span-3">
          {categories.map((category) => (
            <article key={category.id} className="glass rounded-3xl p-5">
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                  <div className="mb-2 h-2 w-16 rounded-full" style={{ background: category.color }} />
                  <h3 className="text-xl font-bold">{category.name}</h3>
                  <p className="text-sm text-slate-400">{category.description}</p>
                  <p className="mt-2 text-sm">
                    {category.numberPrefix}{category.startNumber} to {category.numberPrefix}{category.endNumber}
                    {" · "}
                    {category.isVip || category.isFreeEntry ? "VIP / free" : money(category.price, settings.currency)}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-2xl font-extrabold">{category.soldTickets}/{category.totalTickets}</p>
                  <p className="text-sm text-slate-400">{category.remainingTickets} remaining</p>
                </div>
              </div>
              <div className="mt-4 h-2 overflow-hidden rounded-full bg-white/10">
                <div
                  className="h-full rounded-full"
                  style={{
                    width: `${category.totalTickets ? (category.soldTickets / category.totalTickets) * 100 : 0}%`,
                    background: category.color
                  }}
                />
              </div>
              <div className="mt-4 flex gap-3">
                <button className="btn-ghost" onClick={() => startEdit(category)}>Edit range</button>
                <button className="btn-primary" onClick={() => generate(category.id)}>Generate unique numbers</button>
              </div>
            </article>
          ))}
        </div>
      </div>
    </div>
  );
}
