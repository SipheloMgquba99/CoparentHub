import {
  createContext,
  useState,
  useEffect,
  useCallback,
  useRef,
  type ReactNode,
  type FC,
} from "react";
import type { User, LoginRequest, RegisterRequest, AuthResponse } from "../types";
import * as api from "../api";
import { getToken, setUnauthorizedHandler } from "../api";

interface JwtPayload {
  sub: string;
  email: string;
  exp: number;
}

function decodeJwt(token: string): JwtPayload | null {
  try {
    const payload = token.split(".")[1];
    const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(decoded) as JwtPayload;
  } catch {
    return null;
  }
}

function isTokenExpired(token: string): boolean {
  const p = decodeJwt(token);
  return !p || p.exp * 1000 <= Date.now();
}

function msUntilExpiry(token: string): number {
  const p = decodeJwt(token);
  if (!p) return 0;
  return p.exp * 1000 - Date.now();
}

const PROFILE_KEY = "cp_profile";

interface CachedProfile {
  id: string;
  fullName: string;
  email: string;
}

function cacheProfile(p: CachedProfile): void {
  sessionStorage.setItem(PROFILE_KEY, JSON.stringify(p));
}

function readCachedProfile(): CachedProfile | null {
  try {
    const raw = sessionStorage.getItem(PROFILE_KEY);
    return raw ? (JSON.parse(raw) as CachedProfile) : null;
  } catch {
    return null;
  }
}

function clearCachedProfile(): void {
  sessionStorage.removeItem(PROFILE_KEY);
}

function userFromAuthResponse(res: AuthResponse): User {
  cacheProfile({ id: res.userId, fullName: res.fullName, email: res.email });
  return {
    id: res.userId,
    fullName: res.fullName,
    email: res.email,
  };
}

function userFromStoredToken(token: string): User | null {
  const claims = decodeJwt(token);
  if (!claims) return null;
  const cached = readCachedProfile();
  return {
    id: claims.sub,
    email: claims.email,
    fullName: cached?.id === claims.sub ? cached.fullName : claims.email,
  };
}

export interface AuthContextValue {
  user: User | null;
  loading: boolean;
  login: (req: LoginRequest) => Promise<void>;
  register: (req: RegisterRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export const AuthProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const initialTokenRef = useRef<string | null>(null);
  const [user, setUser] = useState<User | null>(() => {
    const token = getToken();
    if (!token) return null;
    if (isTokenExpired(token)) {
      api.logout();
      clearCachedProfile();
      return null;
    }
    const u = userFromStoredToken(token);
    if (!u) {
      api.logout();
      clearCachedProfile();
      return null;
    }
    initialTokenRef.current = token;
    return u;
  });
  const loading = false;
  const expiryTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  const logout = useCallback(() => {
    if (expiryTimer.current) clearTimeout(expiryTimer.current);
    api.logout();
    clearCachedProfile();
    setUser(null);
  }, []);

  const scheduleExpiryLogout = useCallback((token: string) => {
    if (expiryTimer.current) clearTimeout(expiryTimer.current);
    const ms = msUntilExpiry(token);
    if (ms <= 0) {
      logout();
      return;
    }
    expiryTimer.current = setTimeout(logout, ms);
  }, [logout]);

  useEffect(() => {
    setUnauthorizedHandler(logout);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  useEffect(() => {
    if (initialTokenRef.current) {
      scheduleExpiryLogout(initialTokenRef.current);
    }
  }, [scheduleExpiryLogout]);

  const login = useCallback(async (req: LoginRequest) => {
    const res = await api.login(req);
    setUser(userFromAuthResponse(res));
    scheduleExpiryLogout(res.token);
  }, [scheduleExpiryLogout]);

  const register = useCallback(async (req: RegisterRequest) => {
    const res = await api.register(req);
    setUser(userFromAuthResponse(res));
    scheduleExpiryLogout(res.token);
  }, [scheduleExpiryLogout]);

  return (
    <AuthContext.Provider
      value={{ user, loading, login, register, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
};
