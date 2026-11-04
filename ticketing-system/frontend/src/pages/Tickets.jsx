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
  ticketCount: 100,
  additionalTickets: 0,
  color: "#22d3ee",
  isActive: true
};

function formatTicket(prefix, sequence, endForPad) {
  const pad = Math.max(4, String(endForPad).length);
  return `${prefix || ""}${String(sequence).padStart(pad, "0")}`;
}

export default function Tickets() {
  const [categories, setCategories] = useState([]);
  const [settings, setSettings] = useState({ currency: "LKR" });
  const [form, setForm] = useState(empty);
  const [editing, setEditing] = useState(null);
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
    setEditing(category);
    setForm({
      name: category.name,
      description: category.description,
      price: category.price,
      isVip: category.isVip,
      isFreeEntry: category.isFreeEntry,
      numberPrefix: category.numberPrefix,
      startNumber: category.startNumber,
      ticketCount: category.totalTickets,
      additionalTickets: 0,
      color: category.color,
      isActive: category.isActive
    });
    setError("");
    setMessage("");
  }

  function cancelEdit() {
    setEditing(null);
    setForm(empty);
    setError("");
  }

  async function save(e) {
    e.preventDefault();
    setBusy(true);
    setError("");
    setMessage("");
    const additional = Number(form.additionalTickets || 0);
    const ticketCount = Number(form.ticketCount);
    try {
      const payload = {
        name: form.name,
        description: form.description,
        price: Number(form.price),
        isVip: form.isVip,
        isFreeEntry: form.isFreeEntry,
        numberPrefix: form.numberPrefix,
        startNumber: Number(form.startNumber),
        endNumber: Number(form.startNumber),
        ticketCount: editing ? 0 : ticketCount,
        additionalTickets: editing ? additional : 0,
        color: form.color,
        isActive: form.isActive
      };
      const { data } = editing
        ? await api.put(`/api/ticket-categories/${editing.id}`, payload)
        : await api.post("/api/ticket-categories", payload);

      if (editing && additional > 0) {
        setMessage(`Added ${additional} tickets. ${data.remainingTickets} are available now.`);
      } else if (editing) {
        setMessage("Section updated.");
      } else {
        setMessage(`Section created with ${data.remainingTickets} tickets available.`);
      }
      setForm(empty);
      setEditing(null);
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

  const count = Number(form.ticketCount) || 0;
  const start = Number(form.startNumber) || 1;
  const createEnd = count > 0 ? start + count - 1 : start;
  const extra = Number(form.additionalTickets) || 0;
  const editEnd = editing ? Number(editing.endNumber) + extra : 0;

  return (
    <div>
      <h1 className="page-title">Ticket management</h1>
      <p className="page-sub">Create a section and set how many tickets are available. Add more later and the stock updates everywhere.</p>

      <div className="mt-8 grid gap-6 xl:grid-cols-5">
        <form onSubmit={save} className="glass rounded-3xl p-6 xl:col-span-2">
          <h2 className="text-lg font-bold">{editing ? "Edit section" : "New section"}</h2>
          <input className="field mt-4" placeholder="Section name" required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          <textarea className="field mt-3 min-h-20" placeholder="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          <div className="mt-3 grid grid-cols-2 gap-3">
            <input className="field" type="number" min="0" placeholder="Price" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
            <input className="field" placeholder="Prefix e.g. GEN-" value={form.numberPrefix} onChange={(e) => setForm({ ...form, numberPrefix: e.target.value })} />
          </div>

          {!editing && (
            <>
              <div className="mt-3 grid grid-cols-2 gap-3">
                <label className="block text-xs font-semibold uppercase tracking-wide text-slate-400">
                  Start number
                  <input className="field mt-2" type="number" min="1" required value={form.startNumber} onChange={(e) => setForm({ ...form, startNumber: e.target.value })} />
                </label>
                <label className="block text-xs font-semibold uppercase tracking-wide text-slate-400">
                  How many tickets available
                  <input className="field mt-2" type="number" min="1" max="10000" required value={form.ticketCount} onChange={(e) => setForm({ ...form, ticketCount: e.target.value })} />
                </label>
              </div>
              {count > 0 && (
                <p className="mt-3 text-xs text-cyan-200">
                  This creates {count} available tickets, numbered {formatTicket(form.numberPrefix, start, createEnd)} through {formatTicket(form.numberPrefix, createEnd, createEnd)}.
                </p>
              )}
            </>
          )}

          {editing && (
            <div className="mt-4 rounded-2xl border border-white/10 bg-ink-950/50 p-4">
              <p className="text-sm text-slate-300">
                <span className="text-2xl font-extrabold text-white">{editing.remainingTickets}</span> available now
              </p>
              <p className="mt-1 text-xs text-slate-400">
                {editing.soldTickets} issued · {editing.totalTickets} in this section · {formatTicket(editing.numberPrefix, editing.startNumber, editing.endNumber)} to {formatTicket(editing.numberPrefix, editing.endNumber, editing.endNumber)}
              </p>
              <label className="mt-4 block text-xs font-semibold uppercase tracking-wide text-slate-400">
                Add more tickets
                <input
                  className="field mt-2"
                  type="number"
                  min="0"
                  max="10000"
                  value={form.additionalTickets}
                  onChange={(e) => setForm({ ...form, additionalTickets: e.target.value })}
                />
              </label>
              {extra > 0 && (
                <p className="mt-2 text-xs text-cyan-200">
                  Adds {extra} tickets, {formatTicket(form.numberPrefix, Number(editing.endNumber) + 1, editEnd)} through {formatTicket(form.numberPrefix, editEnd, editEnd)}. Available becomes {editing.remainingTickets + extra}.
                </p>
              )}
            </div>
          )}

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
            <button className="btn-primary flex-1" disabled={busy || (!editing && count < 1)}>
              {busy ? "Saving..." : editing ? "Update section" : "Create section"}
            </button>
            {editing && (
              <button type="button" className="btn-ghost" onClick={cancelEdit}>
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
                    {formatTicket(category.numberPrefix, category.startNumber, category.endNumber)} to {formatTicket(category.numberPrefix, category.endNumber, category.endNumber)}
                    {" · "}
                    {category.isVip || category.isFreeEntry ? "VIP / free" : money(category.price, settings.currency)}
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-2xl font-extrabold">{category.remainingTickets}</p>
                  <p className="text-sm text-slate-400">{category.soldTickets} issued of {category.totalTickets}</p>
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
                <button className="btn-ghost" onClick={() => startEdit(category)}>Edit section</button>
                <button className="btn-primary" onClick={() => generate(category.id)}>Fill missing numbers</button>
              </div>
            </article>
          ))}
        </div>
      </div>
    </div>
  );
}
