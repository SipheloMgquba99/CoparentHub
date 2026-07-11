const BASE = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:44327";
const REQUEST_TIMEOUT_MS = 30_000;

export function apiUrl(path: string): string {
  return `${BASE}/api${path}`;
}

let _token: string | null = sessionStorage.getItem("cp_token");

let _onUnauthorized: (() => void) | null = null;
export function setUnauthorizedHandler(fn: (() => void) | null): void {
  _onUnauthorized = fn;
}

export function setToken(token: string | null): void {
  _token = token;
  if (token) sessionStorage.setItem("cp_token", token);
  else sessionStorage.removeItem("cp_token");
}

export function getToken(): string | null {
  return _token;
}

interface ApiError {
  error?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const isFormData = body instanceof FormData;
  const headers: Record<string, string> = isFormData ? {} : { "Content-Type": "application/json" };
  if (_token) headers["Authorization"] = `Bearer ${_token}`;

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  let res: Response;
  try {
    res = await fetch(apiUrl(path), {
      method,
      headers,
      body: body === undefined ? undefined : isFormData ? (body as FormData) : JSON.stringify(body),
      signal: controller.signal,
    });
  } catch (ex: unknown) {
    if (ex instanceof DOMException && ex.name === "AbortError") {
      throw new Error("The request timed out. Please check your connection and try again.");
    }
    throw new Error("Unable to reach the server. Please check your connection and try again.");
  } finally {
    clearTimeout(timeout);
  }

  if (res.status === 401) {
    setToken(null);
    _onUnauthorized?.();
    throw new Error("Your session has expired. Please sign in again.");
  }

  if (res.status === 429) {
    const retryAfter = res.headers.get("Retry-After");
    throw new Error(
      retryAfter
        ? `Too many requests. Please try again in ${retryAfter} seconds.`
        : "Too many requests. Please slow down and try again shortly."
    );
  }

  if (!res.ok) {
    let message = `Request failed: ${res.status} ${res.statusText}`;
    try {
      const err: ApiError = await res.json();
      message = err.error ?? err.detail ?? err.title ?? message;
      if (err.errors) {
        const flat = Object.values(err.errors).flat().join(" ");
        if (flat) message = flat;
      }
    } catch { }
    throw new Error(message);
  }

  if (res.status === 204) return undefined as T;

  const text = await res.text();
  if (!text) return undefined as T;
  return JSON.parse(text) as T;
}