import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.DEV ? "" : import.meta.env.VITE_API_URL || "http://localhost:5088"
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("el_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !error.config?.url?.includes("/api/auth/login")) {
      localStorage.removeItem("el_token");
      localStorage.removeItem("el_user");
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  }
);

export function apiError(error, fallback = "Something went wrong.") {
  if (!error.response) {
    return "Cannot reach the API. Start the backend with: dotnet watch run";
  }

  const data = error.response.data;
  return data?.message || data?.title || fallback;
}

export default api;
