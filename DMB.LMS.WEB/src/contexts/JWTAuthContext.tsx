import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import http from "../lms/services/http.service";
import type { LocationMembership, LoginResponse } from "../lms/models";
import { firstNameFromToken } from "../lms/utils/sessionUser";

type AuthContextValue = {
  token: string | null;
  isAuthenticated: boolean;
  firstName: string;
  locations: LocationMembership[];
  locationId: string | null;
  currentRole: string;
  setLocationId: (id: string) => void;
  login: (username: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("lms_token"));
  const [locations, setLocations] = useState<LocationMembership[]>(() => {
    const raw = localStorage.getItem("lms_locations");
    return raw ? (JSON.parse(raw) as LocationMembership[]) : [];
  });
  const [locationId, setLocationIdState] = useState<string | null>(() => localStorage.getItem("lms_locationId"));
  const [firstName, setFirstName] = useState(() => localStorage.getItem("lms_firstName") || firstNameFromToken(localStorage.getItem("lms_token")));

  const persistLocations = useCallback((next: LocationMembership[], current?: string) => {
    setLocations(next);
    localStorage.setItem("lms_locations", JSON.stringify(next));
    const selected = current && next.some((l) => l.locationId === current) ? current : next[0]?.locationId ?? null;
    setLocationIdState(selected);
    if (selected) localStorage.setItem("lms_locationId", selected);
    else localStorage.removeItem("lms_locationId");
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    const res = await http.post<LoginResponse>("/auth/login", { username, password });
    localStorage.setItem("lms_token", res.data.token);
    setToken(res.data.token);
    persistLocations(res.data.locations ?? [], res.data.currentLocationId);
    const name = res.data.firstName || firstNameFromToken(res.data.token);
    setFirstName(name);
    if (name) localStorage.setItem("lms_firstName", name);
  }, [persistLocations]);

  const logout = useCallback(async () => {
    try { if (localStorage.getItem("lms_token")) await http.post("/auth/logout"); } catch { /* ignore */ }
    localStorage.removeItem("lms_token");
    localStorage.removeItem("lms_locations");
    localStorage.removeItem("lms_locationId");
    localStorage.removeItem("lms_firstName");
    setToken(null);
    setFirstName("");
    setLocations([]);
    setLocationIdState(null);
    navigate("/login", { replace: true });
  }, [navigate]);

  useEffect(() => {
    const onUnauthorized = () => { void logout(); };
    window.addEventListener("lms:unauthorized", onUnauthorized);
    return () => window.removeEventListener("lms:unauthorized", onUnauthorized);
  }, [logout]);

  const currentRole = locations.find((l) => l.locationId === locationId)?.role ?? "parent";
  const value = useMemo(() => ({
    token, isAuthenticated: Boolean(token), firstName, locations, locationId, currentRole,
    setLocationId: (id: string) => { setLocationIdState(id); localStorage.setItem("lms_locationId", id); },
    login, logout,
  }), [token, firstName, locations, locationId, currentRole, login, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}
