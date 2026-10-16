import { jwtDecode } from "jwt-decode";
import { tokenKeyName } from "~/constants/constants";
import type { IAuthUser, UserRole } from "~/interfaces/IUser";

interface TokenPayload {
  sub: string;
  name: string;
  email: string;
  role: UserRole;
  exp: number;
}

export function getToken() {
  return typeof window === "undefined" ? null : localStorage.getItem(tokenKeyName);
}

export function saveToken(token: string) {
  localStorage.setItem(tokenKeyName, token);
}

export function clearToken() {
  localStorage.removeItem(tokenKeyName);
}

export function readAuthUser(): IAuthUser | null {
  const token = getToken();
  if (!token) return null;

  try {
    const payload = jwtDecode<TokenPayload>(token);
    if (payload.exp * 1000 <= Date.now()) return null;

    return { userId: Number(payload.sub), name: payload.name, email: payload.email, role: payload.role };
  } catch {
    return null;
  }
}
