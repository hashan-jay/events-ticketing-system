import { useState } from "react";
import api, { apiError } from "../api/client";
import { formatDateTime, money, statusLabel } from "../lib/format";

export default function Audit() {
  const [query, setQuery] = useState("");
  const [ticket, setTicket] = useState(null);
  const [error, setError] = useState("");
  const [currency, setCurrency] = useState("LKR");

  async function search(e) {
    e.preventDefault();
    setError("");
    setTicket(null);
    try {
      const settings = await api.get("/api/settings");
      setCurrency(settings.data.currency);
      const { data } = await api.get(`/api/tickets/${encodeURIComponent(query.trim())}`);
      setTicket(data);
    } catch (err) {
      setError(apiError(err, "Ticket not found."));
    }
  }

  return (
    <div>
      <h1 className="page-title">Ticket lookup</h1>
      <p className="page-sub">Audit any unique ticket number for customer, payment, time and issuer details.</p>

      <form onSubmit={search} className="glass mt-8 flex flex-col gap-3 rounded-3xl p-5 sm:flex-row">
        <input
          className="field"
          placeholder="Enter unique ticket number e.g. GEN-1001"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          required
        />
        <button className="btn-primary sm:w-40">Search</button>
      </form>

      {error && <p className="mt-4 rounded-2xl bg-rose-500/15 p-4 text-rose-300">{error}</p>}

      {ticket && (
        <div className="mt-6 grid gap-6 xl:grid-cols-3">
          <section className="glass rounded-3xl p-6">
            <p className="text-xs uppercase tracking-wide text-slate-400">Ticket</p>
            <h2 className="mt-1 text-3xl font-extrabold">{ticket.ticketNumber}</h2>
            <p className="mt-2 text-slate-400">{statusLabel(ticket.status)} · {ticket.category.name}</p>
            <div className="mt-5 space-y-2 text-sm">
              <Line label="Sequence" value={ticket.sequenceNumber} />
              <Line label="Price band" value={ticket.category.isVip || ticket.category.isFreeEntry ? "VIP / free" : money(ticket.category.price, currency)} />
              <Line label="Created" value={formatDateTime(ticket.createdAt)} />
            </div>
          </section>

          <section className="glass rounded-3xl p-6 xl:col-span-2">
            {ticket.transaction ? (
              <>
                <h3 className="text-lg font-bold">Issue record</h3>
                <div className="mt-4 grid gap-3 md:grid-cols-2 text-sm">
                  <Line label="Customer" value={ticket.transaction.customerName || "-"} />
                  <Line label="Phone" value={ticket.transaction.customerPhone || "-"} />
                  <Line label="Payment" value={ticket.transaction.paymentMethodName} />
                  <Line label="Amount" value={money(ticket.transaction.amountCollected, currency)} />
                  <Line label="Issued by" value={ticket.transaction.issuedBy} />
                  <Line label="Issued at" value={formatDateTime(ticket.transaction.issuedAt)} />
                </div>
                {ticket.transaction.notes && <p className="mt-4 text-sm text-slate-300">Notes: {ticket.transaction.notes}</p>}
                <div className="mt-5">
                  <p className="text-xs uppercase tracking-wide text-slate-400">Captured fields</p>
                  <div className="mt-2 grid gap-2 md:grid-cols-2">
                    {ticket.transaction.fieldValues.map((field) => (
                      <Line key={field.customFieldId} label={field.label} value={field.value} />
                    ))}
                  </div>
                </div>
              </>
            ) : (
              <p className="text-slate-400">This ticket has not been issued yet. It is still available in stock.</p>
            )}
          </section>
        </div>
      )}
    </div>
  );
}

function Line({ label, value }) {
  return (
    <div className="rounded-xl bg-ink-950/60 px-3 py-2">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="font-semibold">{value}</p>
    </div>
  );
}
