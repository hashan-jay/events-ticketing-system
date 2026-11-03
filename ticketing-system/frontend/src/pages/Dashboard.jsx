import { useEffect, useState } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from "recharts";
import { CircleDollarSign, Ticket, Sparkles, Clock3 } from "lucide-react";
import api from "../api/client";
import { formatDateTime, money } from "../lib/format";

const cards = [
  { key: "soldTickets", label: "Tickets sold", icon: Ticket, tone: "from-cyan-400 to-sky-500" },
  { key: "remainingTickets", label: "Tickets left", icon: Sparkles, tone: "from-amber-400 to-orange-500" },
  { key: "totalRevenue", label: "Total revenue", icon: CircleDollarSign, tone: "from-emerald-400 to-teal-500", money: true },
  { key: "todaySold", label: "Sold today", icon: Clock3, tone: "from-fuchsia-400 to-pink-500" }
];

export default function Dashboard() {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .get("/api/dashboard")
      .then((res) => setData(res.data))
      .catch(() => setError("Unable to load dashboard analytics."));
  }, []);

  if (error) {
    return <p className="rounded-2xl bg-rose-500/15 p-4 text-rose-300">{error}</p>;
  }

  if (!data) {
    return <p className="text-slate-400">Loading live desk analytics...</p>;
  }

  const hourly = data.hourlyToday.filter((h) => h.tickets > 0 || h.hour >= 8);

  return (
    <div>
      <p className="text-xs uppercase tracking-[0.24em] text-cyan-300">{data.eventName}</p>
      <h1 className="page-title">Command dashboard</h1>
      <p className="page-sub">
        Sold vs remaining stock, revenue by payment channel, and section totals for gate decisions.
      </p>

      <div className="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => {
          const Icon = card.icon;
          const value = card.money ? money(data[card.key], data.currency) : data[card.key];
          return (
            <div key={card.key} className={`rounded-3xl bg-gradient-to-br ${card.tone} p-5 text-ink-950 shadow-lg`}>
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold opacity-80">{card.label}</p>
                <Icon size={20} />
              </div>
              <p className="mt-4 text-3xl font-extrabold">{value}</p>
            </div>
          );
        })}
      </div>

      <div className="mt-4 grid gap-4 md:grid-cols-3">
        <MiniStat label="All tickets" value={data.totalTickets} />
        <MiniStat label="Free / VIP issued" value={data.freePassTickets} />
        <MiniStat label="Today revenue" value={money(data.todayRevenue, data.currency)} />
      </div>

      <div className="mt-6 grid gap-6 xl:grid-cols-2">
        <section className="glass rounded-3xl p-5">
          <h2 className="text-lg font-bold">Revenue by payment method</h2>
          <p className="mb-4 text-sm text-slate-400">Each collection channel, including complimentary VIP passes.</p>
          <div className="grid gap-3 sm:grid-cols-2">
            {data.byPaymentMethod.map((method) => (
              <div key={method.id} className="rounded-2xl border border-white/10 bg-ink-950/50 p-4">
                <div className="mb-3 h-2 rounded-full" style={{ background: method.color }} />
                <p className="text-sm font-semibold">{method.name}</p>
                <p className="mt-1 text-xl font-extrabold">{method.ticketCount} tickets</p>
                <p className="text-sm text-slate-400">
                  {method.collectsRevenue ? money(method.revenue, data.currency) : "No revenue"}
                </p>
              </div>
            ))}
          </div>
        </section>

        <section className="glass rounded-3xl p-5">
          <h2 className="text-lg font-bold">Section stock</h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data.byCategory}>
                <CartesianGrid stroke="rgba(255,255,255,0.06)" vertical={false} />
                <XAxis dataKey="name" stroke="#94a3b8" fontSize={12} />
                <YAxis stroke="#94a3b8" fontSize={12} />
                <Tooltip contentStyle={{ background: "#0B1224", border: "1px solid #334155" }} />
                <Bar dataKey="sold" name="Sold" fill="#22d3ee" radius={[8, 8, 0, 0]} />
                <Bar dataKey="remaining" name="Left" fill="#f59e0b" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </section>
      </div>

      <div className="mt-6 grid gap-6 xl:grid-cols-5">
        <section className="glass rounded-3xl p-5 xl:col-span-3">
          <h2 className="text-lg font-bold">Today hourly sales</h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={hourly}>
                <CartesianGrid stroke="rgba(255,255,255,0.06)" vertical={false} />
                <XAxis dataKey="hour" stroke="#94a3b8" fontSize={12} />
                <YAxis stroke="#94a3b8" fontSize={12} />
                <Tooltip contentStyle={{ background: "#0B1224", border: "1px solid #334155" }} />
                <Bar dataKey="tickets" fill="#a855f7" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </section>
        <section className="glass rounded-3xl p-5 xl:col-span-2">
          <h2 className="text-lg font-bold">Mix by method</h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={data.byPaymentMethod} dataKey="ticketCount" nameKey="name" innerRadius={50} outerRadius={90}>
                  {data.byPaymentMethod.map((entry) => (
                    <Cell key={entry.id} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ background: "#0B1224", border: "1px solid #334155" }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </section>
      </div>

      <section className="glass mt-6 rounded-3xl p-5">
        <h2 className="text-lg font-bold">Latest issues</h2>
        <div className="mt-4 overflow-x-auto">
          <table className="w-full min-w-[720px] text-left text-sm">
            <thead className="text-slate-400">
              <tr>
                <th className="pb-3">Ticket</th>
                <th className="pb-3">Customer</th>
                <th className="pb-3">Method</th>
                <th className="pb-3">Amount</th>
                <th className="pb-3">When</th>
              </tr>
            </thead>
            <tbody>
              {data.recentTransactions.length === 0 && (
                <tr>
                  <td colSpan={5} className="py-6 text-slate-500">No tickets issued yet. Use Issue Ticket to start the gate.</td>
                </tr>
              )}
              {data.recentTransactions.map((tx) => (
                <tr key={tx.id} className="border-t border-white/5">
                  <td className="py-3 font-semibold">{tx.ticketNumber}</td>
                  <td className="py-3">{tx.customerName || "-"}</td>
                  <td className="py-3">{tx.paymentMethodName}</td>
                  <td className="py-3">{money(tx.amountCollected, data.currency)}</td>
                  <td className="py-3 text-slate-400">{formatDateTime(tx.issuedAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

function MiniStat({ label, value }) {
  return (
    <div className="glass rounded-2xl px-5 py-4">
      <p className="text-xs uppercase tracking-wide text-slate-400">{label}</p>
      <p className="mt-1 text-2xl font-extrabold">{value}</p>
    </div>
  );
}
