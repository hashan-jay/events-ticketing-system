import { useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { apiError } from "../api/client";

export default function Login() {
  const { user, ready, login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("Admin@123");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  if (ready && user) {
    return <Navigate to="/" replace />;
  }

  async function onSubmit(e) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await login(username, password);
      navigate("/");
    } catch (err) {
      setError(apiError(err, "Invalid username or password."));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="relative grid min-h-screen place-items-center overflow-hidden bg-ink-950 px-4">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,rgba(34,211,238,0.18),transparent_30%),radial-gradient(circle_at_80%_10%,rgba(244,63,94,0.16),transparent_24%),radial-gradient(circle_at_70%_80%,rgba(168,85,247,0.16),transparent_28%)]" />
      <form onSubmit={onSubmit} className="glass relative w-full max-w-md rounded-3xl p-8">
        <p className="text-xs uppercase tracking-[0.28em] text-cyan-300">EventsLab 2026</p>
        <h1 className="mt-2 text-3xl font-extrabold">Gate Control Login</h1>
        <p className="mt-2 text-sm text-slate-400">
          Fast-pace ticketing desk for cash, online, card, transfer and VIP free-pass sales.
        </p>
        <label className="mt-6 block text-xs font-semibold uppercase tracking-wide text-slate-400">Username</label>
        <input className="field mt-2" name="username" autoComplete="username" value={username} onChange={(e) => setUsername(e.target.value)} autoFocus />
        <label className="mt-4 block text-xs font-semibold uppercase tracking-wide text-slate-400">Password</label>
        <input className="field mt-2" name="password" autoComplete="current-password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        {error && <p className="mt-4 rounded-xl bg-rose-500/15 px-3 py-2 text-sm text-rose-300">{error}</p>}
        <button className="btn-primary mt-6 w-full" disabled={loading}>
          {loading ? "Signing in..." : "Enter desk"}
        </button>
        <p className="mt-4 text-center text-xs text-slate-500">Default admin / Admin@123</p>
      </form>
    </div>
  );
}
