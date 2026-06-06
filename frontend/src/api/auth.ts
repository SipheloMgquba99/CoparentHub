import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth'
import { request, setToken } from './http';

export async function login(req: LoginRequest): Promise<AuthResponse> {
  const data = await request<AuthResponse>("POST", "/auth/login", req);
  setToken(data.token);
  return data;
}

export async function register(req: RegisterRequest): Promise<AuthResponse> {
  const data = await request<AuthResponse>("POST", "/auth/register", req);
  setToken(data.token);
  return data;
}

export function logout(): void {
  setToken(null);
}
