import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import http from "../lms/services/http.service";
import type { AuthProfile, LocationMembership, LoginResponse } from "../lms/models";
import { firstNameFromToken, isSuperAdminFromToken, userIdFromToken } from "../lms/utils/sessionUser";

type AuthContextValue = {
  token: string | null;
  isAuthenticated: boolean;
  firstName: string;
  userId: string;
  isSuperAdmin: boolean;
  locations: LocationMembership[];
  locationId: string | null;
  currentRole: string;
  setLocationId: (id: string) => void;
  login: (username: string, password: string) => Promise<void>;
  acceptSession: (token: string, locations?: LocationMembership[], currentLocationId?: string, firstName?: string) => Promise<void>;
  refreshProfile: () => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

function storedFlag(key: string) {
  return localStorage.getItem(key) === "true";
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("lms_token"));
  const [locations, setLocations] = useState<LocationMembership[]>(() => {
    const raw = localStorage.getItem("lms_locations");
    return raw ? (JSON.parse(raw) as LocationMembership[]) : [];
  });
  const [locationId, setLocationIdState] = useState<string | null>(() => localStorage.getItem("lms_locationId"));
  const [firstName, setFirstName] = useState(() => localStorage.getItem("lms_firstName") || firstNameFromToken(localStorage.getItem("lms_token")));
  const [userId, setUserId] = useState(() => localStorage.getItem("lms_userId") || userIdFromToken(localStorage.getItem("lms_token")));
  const [isSuperAdmin, setIsSuperAdmin] = useState(() => storedFlag("lms_isSuperAdmin") || isSuperAdminFromToken(localStorage.getItem("lms_token")));

  const persistProfile = useCallback((profile: Partial<AuthProfile> & { firstName?: string }, nextToken?: string | null) => {
    const name = (profile.firstName ?? "").trim() || firstNameFromToken(nextToken ?? token);
    setFirstName(name);
    if (name) localStorage.setItem("lms_firstName", name);
    else localStorage.removeItem("lms_firstName");

    const nextUserId = profile.userId || userIdFromToken(nextToken ?? token);
    setUserId(nextUserId);
    if (nextUserId) localStorage.setItem("lms_userId", nextUserId);
    else localStorage.removeItem("lms_userId");

    const nextSuper = typeof profile.isSuperAdmin === "boolean"
      ? profile.isSuperAdmin
      : isSuperAdminFromToken(nextToken ?? token);
    setIsSuperAdmin(nextSuper);
    if (nextSuper) localStorage.setItem("lms_isSuperAdmin", "true");
    else localStorage.removeItem("lms_isSuperAdmin");
  }, [token]);

  const persistLocations = useCallback((next: LocationMembership[], current?: string) => {
    setLocations(next);
    localStorage.setItem("lms_locations", JSON.stringify(next));
    const selected = current && next.some((l) => l.locationId === current) ? current : next[0]?.locationId ?? null;
    setLocationIdState(selected);
    if (selected) localStorage.setItem("lms_locationId", selected);
    else localStorage.removeItem("lms_locationId");
  }, []);

  const refreshProfile = useCallback(async () => {
    try {
      const res = await http.get<AuthProfile>("/auth/me", { skipLoading: true });
      persistProfile(res.data, localStorage.getItem("lms_token"));
    } catch {
      persistProfile({}, localStorage.getItem("lms_token"));
    }
  }, [persistProfile]);

  const login = useCallback(async (username: string, password: string) => {
    const res = await http.post<LoginResponse>("/auth/login", { username, password });
    localStorage.setItem("lms_token", res.data.token);
    setToken(res.data.token);
    persistLocations(res.data.locations ?? [], res.data.currentLocationId);
    persistProfile({
      firstName: res.data.firstName,
      isSuperAdmin: Boolean(res.data.isSuperAdmin),
      userId: userIdFromToken(res.data.token),
    }, res.data.token);
  }, [persistLocations, persistProfile]);

  const acceptSession = useCallback(async (
    nextToken: string,
    nextLocations?: LocationMembership[],
    currentLocationId?: string,
    name?: string
  ) => {
    localStorage.setItem("lms_token", nextToken);
    setToken(nextToken);
    persistProfile({
      firstName: name,
      isSuperAdmin: isSuperAdminFromToken(nextToken),
      userId: userIdFromToken(nextToken),
    }, nextToken);

    if (nextLocations && nextLocations.length > 0) {
      persistLocations(nextLocations, currentLocationId);
    } else {
      try {
        const res = await http.get<LocationMembership[]>("/location/list");
        persistLocations(res.data ?? [], currentLocationId);
      } catch {
        persistLocations([], currentLocationId);
      }
    }

    await refreshProfile();
  }, [persistLocations, persistProfile, refreshProfile]);

  const logout = useCallback(async () => {
    try { if (localStorage.getItem("lms_token")) await http.post("/auth/logout"); } catch { /* ignore */ }
    localStorage.removeItem("lms_token");
    localStorage.removeItem("lms_locations");
    localStorage.removeItem("lms_locationId");
    localStorage.removeItem("lms_firstName");
    localStorage.removeItem("lms_userId");
    localStorage.removeItem("lms_isSuperAdmin");
    setToken(null);
    setFirstName("");
    setUserId("");
    setIsSuperAdmin(false);
    setLocations([]);
    setLocationIdState(null);
    navigate("/login", { replace: true });
  }, [navigate]);

  useEffect(() => {
    const onUnauthorized = () => { void logout(); };
    window.addEventListener("lms:unauthorized", onUnauthorized);
    return () => window.removeEventListener("lms:unauthorized", onUnauthorized);
  }, [logout]);

  useEffect(() => {
    if (!token) return;
    void refreshProfile();
  }, [token, refreshProfile]);

  const currentRole = locations.find((l) => l.locationId === locationId)?.role ?? "parent";
  const value = useMemo(() => ({
    token, isAuthenticated: Boolean(token), firstName, userId, isSuperAdmin, locations, locationId, currentRole,
    setLocationId: (id: string) => { setLocationIdState(id); localStorage.setItem("lms_locationId", id); },
    login, acceptSession, refreshProfile, logout,
  }), [token, firstName, userId, isSuperAdmin, locations, locationId, currentRole, login, acceptSession, refreshProfile, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}
