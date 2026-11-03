export function money(value, currency = "LKR") {
  const amount = Number(value || 0);
  return `${currency} ${amount.toLocaleString(undefined, { maximumFractionDigits: 0 })}`;
}

export function formatDateTime(value) {
  if (!value) return "-";
  return new Date(value).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit"
  });
}

export function statusLabel(status) {
  if (status === 1 || status === "Issued") return "Issued";
  if (status === 2 || status === "Voided") return "Voided";
  return "Available";
}
