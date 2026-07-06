export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  userId: string;
  token: string;
  fullName: string;
  email: string;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
}
