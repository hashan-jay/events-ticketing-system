import { useEffect, useState } from "react";
import api from "../api/client";
import { formatDateTime, money } from "../lib/format";

export default function Transactions() {
  const [rows, setRows] = useState([]);
  const [methods, setMethods] = useState([]);
  const [categories, setCategories] = useState([]);
  const [settings, setSettings] = useState({ currency: "LKR" });
  const [filters, setFilters] = useState({ search: "", paymentMethodId: "", categoryId: "" });

  async function load() {
    const params = { take: 200 };
    if (filters.paymentMethodId) params.paymentMethodId = filters.paymentMethodId;
    if (filters.categoryId) params.categoryId = filters.categoryId;
    if (filters.search) params.search = filters.search;
    const { data } = await api.get("/api/transactions", { params });
    setRows(data);
  }

  useEffect(() => {
    Promise.all([api.get("/api/payment-methods"), api.get("/api/ticket-categories"), api.get("/api/settings")])
      .then(([m, c, s]) => {
        setMethods(m.data);
        setCategories(c.data);
        setSettings(s.data);
      });
  }, []);

  useEffect(() => {
    load();
  }, [filters.paymentMethodId, filters.categoryId]);

  const total = rows.reduce((sum, row) => sum + row.amountCollected, 0);

  return (
    <div>
      <h1 className="page-title">Transactions</h1>
      <p className="page-sub">Every issued ticket with time, customer, method and collected amount.</p>

      <div className="mt-6 grid gap-3 md:grid-cols-4">
        <input
          className="field md:col-span-2"
          placeholder="Search ticket, name or phone"
          value={filters.search}
          onChange={(e) => setFilters({ ...filters, search: e.target.value })}
          onKeyDown={(e) => e.key === "Enter" && load()}
        />
        <select className="field" value={filters.paymentMethodId} onChange={(e) => setFilters({ ...filters, paymentMethodId: e.target.value })}>
          <option value="">All payment methods</option>
          {methods.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
        </select>
        <select className="field" value={filters.categoryId} onChange={(e) => setFilters({ ...filters, categoryId: e.target.value })}>
          <option value="">All sections</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
      </div>

      <div className="mt-4 grid gap-3 md:grid-cols-3">
        <Stat label="Matching sales" value={rows.length} />
        <Stat label="Collected in view" value={money(total, settings.currency)} />
        <Stat label="Complimentary in view" value={rows.filter((r) => r.amountCollected === 0).length} />
      </div>

      <section className="glass mt-6 overflow-x-auto rounded-3xl p-5">
        <table className="w-full min-w-[900px] text-left text-sm">
          <thead className="text-slate-400">
            <tr>
              <th className="pb-3">When</th>
              <th className="pb-3">Ticket</th>
              <th className="pb-3">Customer</th>
              <th className="pb-3">Section</th>
              <th className="pb-3">Method</th>
              <th className="pb-3">Amount</th>
              <th className="pb-3">Issuer</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.id} className="border-t border-white/5">
                <td className="py-3 text-slate-400">{formatDateTime(row.issuedAt)}</td>
                <td className="py-3 font-semibold">{row.ticketNumber}</td>
                <td className="py-3">
                  <p>{row.customerName || "-"}</p>
                  <p className="text-xs text-slate-500">{row.customerPhone}</p>
                </td>
                <td className="py-3">{row.categoryName}</td>
                <td className="py-3">{row.paymentMethodName}</td>
                <td className="py-3">{money(row.amountCollected, settings.currency)}</td>
                <td className="py-3">{row.issuedBy}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}

function Stat({ label, value }) {
  return (
    <div className="glass rounded-2xl px-5 py-4">
      <p className="text-xs uppercase tracking-wide text-slate-400">{label}</p>
      <p className="mt-1 text-2xl font-extrabold">{value}</p>
    </div>
  );
}
