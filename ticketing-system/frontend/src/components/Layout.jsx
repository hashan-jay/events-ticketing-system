import { NavLink, Outlet, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  Ticket,
  ScanLine,
  Receipt,
  Settings,
  LogOut,
  Zap,
  ClipboardList
} from "lucide-react";
import { useAuth } from "../context/AuthContext";

const links = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/issue", label: "Issue Ticket", icon: Zap },
  { to: "/tickets", label: "Ticket Management", icon: Ticket },
  { to: "/audit", label: "Ticket Lookup", icon: ScanLine },
  { to: "/audit-log", label: "Audit log", icon: ClipboardList },
  { to: "/transactions", label: "Transactions", icon: Receipt },
  { to: "/settings", label: "Settings", icon: Settings }
];

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-ink-950 text-slate-100">
      <div className="pointer-events-none fixed inset-0 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.12),transparent_28%),radial-gradient(circle_at_80%_20%,rgba(168,85,247,0.16),transparent_24%)]" />
      <aside className="fixed inset-y-0 left-0 z-20 flex w-72 flex-col border-r border-white/10 bg-ink-900/90 backdrop-blur-xl">
        <div className="px-6 pb-4 pt-7">
          <div className="flex items-center gap-3">
            <div className="grid h-11 w-11 place-items-center rounded-2xl bg-gradient-to-br from-cyan-400 to-fuchsia-500 text-ink-950 font-extrabold">
              EL
            </div>
            <div>
              <p className="text-sm uppercase tracking-[0.22em] text-cyan-300">EventsLab</p>
              <p className="text-lg font-bold">Ticketing Desk</p>
            </div>
          </div>
        </div>
        <nav className="flex-1 space-y-1 overflow-y-auto px-3">
          {links.map((link) => {
            const Icon = link.icon;
            return (
              <NavLink
                key={link.to}
                to={link.to}
                end={link.to === "/" || link.to === "/audit"}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition ${
                    isActive
                      ? "bg-white/10 text-white shadow-glow"
                      : "text-slate-400 hover:bg-white/5 hover:text-white"
                  }`
                }
              >
                <Icon size={18} />
                {link.label}
              </NavLink>
            );
          })}
        </nav>
        <div className="border-t border-white/10 p-4">
          <p className="truncate text-sm font-semibold">{user?.fullName}</p>
          <p className="mb-3 text-xs text-slate-400">{user?.role} · {user?.username}</p>
          <button
            className="btn-ghost w-full"
            onClick={async () => {
              await logout();
              navigate("/login");
            }}
          >
            <LogOut size={16} />
            Sign out
          </button>
        </div>
      </aside>
      <main className="relative z-10 ml-72 min-h-screen p-8">
        <Outlet />
      </main>
    </div>
  );
}
