import { createContext, useContext, useEffect, useMemo, useState } from "react";
import api from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const raw = localStorage.getItem("el_user");
    return raw ? JSON.parse(raw) : null;
  });
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("el_token");
    if (!token) {
      setReady(true);
      return;
    }

    api
      .get("/api/auth/me")
      .then((res) => {
        setUser(res.data);
        localStorage.setItem("el_user", JSON.stringify(res.data));
      })
      .catch(() => {
        localStorage.removeItem("el_token");
        localStorage.removeItem("el_user");
        setUser(null);
      })
      .finally(() => setReady(true));
  }, []);

  const value = useMemo(
    () => ({
      user,
      ready,
      async login(username, password) {
        const { data } = await api.post("/api/auth/login", {
          username: username.trim(),
          password: password.trim()
        });
        localStorage.setItem("el_token", data.token);
        localStorage.setItem("el_user", JSON.stringify(data.user));
        setUser(data.user);
        return data.user;
      },
      async logout() {
        try {
          await api.post("/api/auth/logout");
        } catch {
          // Still clear the local session if the API cannot record the sign-out.
        }
        localStorage.removeItem("el_token");
        localStorage.removeItem("el_user");
        setUser(null);
      }
    }),
    [user, ready]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
