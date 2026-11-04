import { useEffect, useState } from "react";
import api, { apiError } from "../api/client";
import { formatDateTime } from "../lib/format";

const actions = [
  { value: "", label: "All actions" },
  { value: "login.succeeded", label: "Sign in" },
  { value: "login.failed", label: "Failed sign in" },
  { value: "logout", label: "Sign out" },
  { value: "ticket.issued", label: "Ticket issued" },
  { value: "ticket.looked_up", label: "Ticket lookup" },
  { value: "category.created", label: "Section created" },
  { value: "category.updated", label: "Section updated" },
  { value: "tickets.generated", label: "Tickets generated" },
  { value: "payment_method.created", label: "Payment method created" },
  { value: "payment_method.updated", label: "Payment method updated" },
  { value: "custom_field.created", label: "Field created" },
  { value: "custom_field.updated", label: "Field updated" },
  { value: "settings.updated", label: "Settings updated" }
];

const labels = Object.fromEntries(actions.filter((item) => item.value).map((item) => [item.value, item.label]));

export default function AuditLog() {
  const [rows, setRows] = useState([]);
  const [search, setSearch] = useState("");
  const [query, setQuery] = useState("");
  const [action, setAction] = useState("");
  const [error, setError] = useState("");

  async function load(nextQuery = query, nextAction = action) {
    setError("");
    const params = { take: 300 };
    if (nextQuery) params.search = nextQuery;
    if (nextAction) params.action = nextAction;
    const { data } = await api.get("/api/audit-logs", { params });
    setRows(data);
  }

  useEffect(() => {
    load().catch(() => setError("Unable to load the audit log."));
  }, [query, action]);

  function onSearch(e) {
    e.preventDefault();
    const next = search.trim();
    setQuery(next);
    if (next === query) {
      load(next, action).catch(() => setError("Unable to load the audit log."));
    }
  }

  return (
    <div>
      <h1 className="page-title">Audit log</h1>
      <p className="page-sub">Every sign-in, ticket issue, stock change, lookup, and settings update is recorded here.</p>

      <form onSubmit={onSearch} className="mt-6 grid gap-3 md:grid-cols-4">
        <input
          className="field md:col-span-2"
          placeholder="Search user, ticket, or what happened"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select className="field" value={action} onChange={(e) => setAction(e.target.value)}>
          {actions.map((item) => (
            <option key={item.value || "all"} value={item.value}>{item.label}</option>
          ))}
        </select>
        <button className="btn-primary">Search</button>
      </form>

      {error && <p className="mt-4 rounded-2xl bg-rose-500/15 p-4 text-rose-300">{error}</p>}

      <section className="glass mt-6 overflow-x-auto rounded-3xl p-5">
        <p className="mb-4 text-sm text-slate-400">{rows.length} entries</p>
        <table className="w-full min-w-[860px] text-left text-sm">
          <thead className="text-slate-400">
            <tr>
              <th className="pb-3">When</th>
              <th className="pb-3">Who</th>
              <th className="pb-3">Action</th>
              <th className="pb-3">What happened</th>
            </tr>
          </thead>
          <tbody>
            {rows.length === 0 && (
              <tr>
                <td colSpan={4} className="border-t border-white/5 py-8 text-slate-400">
                  No activity recorded yet.
                </td>
              </tr>
            )}
            {rows.map((row) => (
              <tr key={row.id} className="border-t border-white/5 align-top">
                <td className="py-3 pr-4 whitespace-nowrap">{formatDateTime(row.occurredAt)}</td>
                <td className="py-3 pr-4 font-semibold">{row.username}</td>
                <td className="py-3 pr-4">
                  <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-semibold">
                    {labels[row.action] || row.action}
                  </span>
                </td>
                <td className="py-3">
                  <p>{row.summary}</p>
                  {row.details && <p className="mt-1 text-xs text-slate-400">{row.details}</p>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}
