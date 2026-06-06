import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
  type FC,
} from "react";
import type { User, LoginRequest, RegisterRequest } from "../types";
import * as api from "../api";
import { getToken } from "../api";

// --- JWT decoding helpers ---
interface JwtPayload {
  sub: string;
  email: string;
  fullName?: string;
  familyId?: string;
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

function userFromToken(token: string): User | null {
  const p = decodeJwt(token);
  if (!p) return null;
  if (p.exp * 1000 < Date.now()) return null;
  return {
    id: p.sub,
    email: p.email,
    fullName: p.fullName ?? p.email,
    familyId: p.familyId ?? null,
  };
}

// --- Auth context ---
interface AuthContextValue {
  user: User | null;
  loading: boolean; // renamed from isLoading
  login: (req: LoginRequest) => Promise<void>;
  register: (req: RegisterRequest) => Promise<void>;
  logout: () => void;
  refreshUser: (familyId: string) => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export const useAuth = (): AuthContextValue => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
};

// --- AuthProvider component ---
export const AuthProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  // Initialize user from token on mount
  useEffect(() => {
    const token = getToken();
    if (token) {
      const u = userFromToken(token);
      if (u) setUser(u);
      else api.logout();
    }
    setLoading(false);
  }, []);

  const login = useCallback(async (req: LoginRequest) => {
    const { token, user: u } = await api.login(req);
    setUser(u ?? userFromToken(token));
  }, []);

  const register = useCallback(async (req: RegisterRequest) => {
    const { token, user: u } = await api.register(req);
    setUser(u ?? userFromToken(token));
  }, []);

  const logout = useCallback(() => {
    api.logout();
    setUser(null);
  }, []);

  const refreshUser = useCallback((familyId: string) => {
    setUser((prev) => (prev ? { ...prev, familyId } : prev));
  }, []);

  return (
    <AuthContext.Provider
      value={{ user, loading, login, register, logout, refreshUser }}
    >
      {children}
    </AuthContext.Provider>
  );
};