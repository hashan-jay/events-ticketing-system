import { useEffect, useState } from "react";
import api, { apiError } from "../api/client";

const emptyMethod = {
  name: "",
  code: "",
  description: "",
  collectsRevenue: true,
  isActive: true,
  color: "#22d3ee",
  displayOrder: 10
};

const emptyField = {
  label: "",
  fieldKey: "",
  fieldType: "text",
  isRequired: false,
  isActive: true,
  displayOrder: 10
};

export default function Settings() {
  const [settings, setSettings] = useState({ eventName: "", currency: "LKR", freeEntryEnabled: true });
  const [methods, setMethods] = useState([]);
  const [fields, setFields] = useState([]);
  const [methodForm, setMethodForm] = useState(emptyMethod);
  const [editingMethod, setEditingMethod] = useState(null);
  const [fieldForm, setFieldForm] = useState(emptyField);
  const [editingField, setEditingField] = useState(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function load() {
    const [s, m, f] = await Promise.all([
      api.get("/api/settings"),
      api.get("/api/payment-methods"),
      api.get("/api/custom-fields")
    ]);
    setSettings(s.data);
    setMethods(m.data);
    setFields(f.data);
  }

  useEffect(() => {
    load().catch(() => setError("Unable to load settings."));
  }, []);

  async function saveSettings(e) {
    e.preventDefault();
    setError("");
    try {
      const { data } = await api.put("/api/settings", settings);
      setSettings(data);
      setMessage("General settings saved.");
    } catch (err) {
      setError(apiError(err, "Could not save settings."));
    }
  }

  async function saveMethod(e) {
    e.preventDefault();
    try {
      if (editingMethod) {
        await api.put(`/api/payment-methods/${editingMethod}`, methodForm);
      } else {
        await api.post("/api/payment-methods", methodForm);
      }
      setMethodForm(emptyMethod);
      setEditingMethod(null);
      setMessage("Payment method saved.");
      await load();
    } catch (err) {
      setError(apiError(err, "Could not save payment method."));
    }
  }

  async function saveField(e) {
    e.preventDefault();
    try {
      if (editingField) {
        await api.put(`/api/custom-fields/${editingField}`, fieldForm);
      } else {
        await api.post("/api/custom-fields", fieldForm);
      }
      setFieldForm(emptyField);
      setEditingField(null);
      setMessage("Customer field saved.");
      await load();
    } catch (err) {
      setError(apiError(err, "Could not save customer field."));
    }
  }

  return (
    <div>
      <h1 className="page-title">Settings</h1>
      <p className="page-sub">Payment methods, free-entry toggle, and extra customer fields collected at the gate.</p>
      {message && <p className="mt-4 rounded-2xl bg-emerald-500/15 p-3 text-emerald-300">{message}</p>}
      {error && <p className="mt-4 rounded-2xl bg-rose-500/15 p-3 text-rose-300">{error}</p>}

      <form onSubmit={saveSettings} className="glass mt-6 rounded-3xl p-6">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h2 className="text-lg font-bold">Event & free entry</h2>
            <p className="text-sm text-slate-400">Turn off complimentary tickets when VIP passes should not be issued.</p>
          </div>
          <label className="flex items-center gap-3 rounded-2xl bg-ink-950/70 px-4 py-3">
            <span className="text-sm font-semibold">Free / VIP entry</span>
            <button
              type="button"
              onClick={() => setSettings((prev) => ({ ...prev, freeEntryEnabled: !prev.freeEntryEnabled }))}
              className={`relative h-8 w-14 rounded-full transition ${settings.freeEntryEnabled ? "bg-cyan-400" : "bg-slate-600"}`}
              aria-label="Toggle free entry"
            >
              <span className={`absolute top-1 h-6 w-6 rounded-full bg-ink-950 transition ${settings.freeEntryEnabled ? "left-7" : "left-1"}`} />
            </button>
          </label>
        </div>
        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <input className="field" value={settings.eventName} onChange={(e) => setSettings({ ...settings, eventName: e.target.value })} placeholder="Event name" />
          <input className="field" value={settings.currency} onChange={(e) => setSettings({ ...settings, currency: e.target.value })} placeholder="Currency" />
        </div>
        <button className="btn-primary mt-4">Save general settings</button>
      </form>

      <div className="mt-6 grid gap-6 xl:grid-cols-2">
        <section className="glass rounded-3xl p-6">
          <h2 className="text-lg font-bold">Payment methods</h2>
          <form onSubmit={saveMethod} className="mt-4 space-y-3">
            <input className="field" placeholder="Name" required value={methodForm.name} onChange={(e) => setMethodForm({ ...methodForm, name: e.target.value })} />
            <input className="field" placeholder="Code" required value={methodForm.code} onChange={(e) => setMethodForm({ ...methodForm, code: e.target.value })} />
            <textarea className="field min-h-16" placeholder="Description" value={methodForm.description} onChange={(e) => setMethodForm({ ...methodForm, description: e.target.value })} />
            <div className="flex flex-wrap items-center gap-3">
              <input type="color" className="h-11 w-14 rounded-lg border border-white/10 bg-transparent" value={methodForm.color} onChange={(e) => setMethodForm({ ...methodForm, color: e.target.value })} />
              <label className="text-sm"><input type="checkbox" className="mr-2" checked={methodForm.collectsRevenue} onChange={(e) => setMethodForm({ ...methodForm, collectsRevenue: e.target.checked })} />Collects revenue</label>
              <label className="text-sm"><input type="checkbox" className="mr-2" checked={methodForm.isActive} onChange={(e) => setMethodForm({ ...methodForm, isActive: e.target.checked })} />Active</label>
            </div>
            <div className="flex gap-3">
              <button className="btn-primary">{editingMethod ? "Update method" : "Add method"}</button>
              {editingMethod && <button type="button" className="btn-ghost" onClick={() => { setEditingMethod(null); setMethodForm(emptyMethod); }}>Cancel</button>}
            </div>
          </form>
          <div className="mt-5 space-y-3">
            {methods.map((method) => (
              <button
                key={method.id}
                type="button"
                className="w-full rounded-2xl border border-white/10 bg-ink-950/50 p-4 text-left"
                onClick={() => {
                  setEditingMethod(method.id);
                  setMethodForm({
                    name: method.name,
                    code: method.code,
                    description: method.description,
                    collectsRevenue: method.collectsRevenue,
                    isActive: method.isActive,
                    color: method.color,
                    displayOrder: method.displayOrder
                  });
                }}
              >
                <div className="mb-2 h-2 w-14 rounded-full" style={{ background: method.color }} />
                <p className="font-semibold">{method.name}</p>
                <p className="text-xs text-slate-400">{method.code} · {method.collectsRevenue ? "Revenue" : "Free pass"}</p>
              </button>
            ))}
          </div>
        </section>

        <section className="glass rounded-3xl p-6">
          <h2 className="text-lg font-bold">Customer fields</h2>
          <form onSubmit={saveField} className="mt-4 space-y-3">
            <input className="field" placeholder="Label" required value={fieldForm.label} onChange={(e) => setFieldForm({ ...fieldForm, label: e.target.value, fieldKey: fieldForm.fieldKey || e.target.value })} />
            <input className="field" placeholder="Field key" required value={fieldForm.fieldKey} onChange={(e) => setFieldForm({ ...fieldForm, fieldKey: e.target.value })} />
            <select className="field" value={fieldForm.fieldType} onChange={(e) => setFieldForm({ ...fieldForm, fieldType: e.target.value })}>
              <option value="text">Text</option>
              <option value="phone">Phone</option>
              <option value="email">Email</option>
              <option value="number">Number</option>
              <option value="textarea">Long text</option>
            </select>
            <div className="flex gap-4 text-sm">
              <label><input type="checkbox" className="mr-2" checked={fieldForm.isRequired} onChange={(e) => setFieldForm({ ...fieldForm, isRequired: e.target.checked })} />Required</label>
              <label><input type="checkbox" className="mr-2" checked={fieldForm.isActive} onChange={(e) => setFieldForm({ ...fieldForm, isActive: e.target.checked })} />Active</label>
            </div>
            <div className="flex gap-3">
              <button className="btn-primary">{editingField ? "Update field" : "Add field"}</button>
              {editingField && <button type="button" className="btn-ghost" onClick={() => { setEditingField(null); setFieldForm(emptyField); }}>Cancel</button>}
            </div>
          </form>
          <div className="mt-5 space-y-3">
            {fields.map((field) => (
              <button
                key={field.id}
                type="button"
                className="w-full rounded-2xl border border-white/10 bg-ink-950/50 p-4 text-left"
                onClick={() => {
                  setEditingField(field.id);
                  setFieldForm({
                    label: field.label,
                    fieldKey: field.fieldKey,
                    fieldType: field.fieldType,
                    isRequired: field.isRequired,
                    isActive: field.isActive,
                    displayOrder: field.displayOrder
                  });
                }}
              >
                <p className="font-semibold">{field.label}</p>
                <p className="text-xs text-slate-400">{field.fieldType} · {field.isRequired ? "Required" : "Optional"}</p>
              </button>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
}
