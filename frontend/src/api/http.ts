const BASE = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:44327";
let _token: string | null = sessionStorage.getItem("cp_token");

export function setToken(token: string | null): void {
  _token = token;
  if (token) sessionStorage.setItem("cp_token", token);
  else sessionStorage.removeItem("cp_token");
}

export function getToken(): string | null {
  return _token;
}

interface ApiError {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (_token) headers["Authorization"] = `Bearer ${_token}`;

  const res = await fetch(`${BASE}/api${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!res.ok) {
    let message = `Request failed: ${res.status} ${res.statusText}`;
    try {
      const err: ApiError = await res.json();
      message = err.detail ?? err.title ?? message;
      if (err.errors) {
        const flat = Object.values(err.errors).flat().join(" ");
        if (flat) message = flat;
      }
    } catch { /* ignore parse error */ }
    throw new Error(message);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
