import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./context/AuthContext";
import Layout from "./components/Layout";
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import IssueTicket from "./pages/IssueTicket";
import Tickets from "./pages/Tickets";
import Audit from "./pages/Audit";
import Transactions from "./pages/Transactions";
import Settings from "./pages/Settings";

function Protected({ children }) {
  const { user, ready } = useAuth();
  if (!ready) {
    return (
      <div className="grid min-h-screen place-items-center bg-ink-950 text-slate-300">
        Loading EventsLab...
      </div>
    );
  }
  if (!user) {
    return <Navigate to="/login" replace />;
  }
  return children;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        element={
          <Protected>
            <Layout />
          </Protected>
        }
      >
        <Route path="/" element={<Dashboard />} />
        <Route path="/issue" element={<IssueTicket />} />
        <Route path="/tickets" element={<Tickets />} />
        <Route path="/audit" element={<Audit />} />
        <Route path="/transactions" element={<Transactions />} />
        <Route path="/settings" element={<Settings />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
