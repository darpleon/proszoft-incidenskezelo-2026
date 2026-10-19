import { createContext, useState, type ReactNode } from "react";
import type { IAuthUser } from "~/interfaces/IUser";
import { readAuthUser } from "~/lib/authToken";

interface AuthContextValue {
  user: IAuthUser | null;
  setUser: (user: IAuthUser | null) => void;
}

export const AuthContext = createContext<AuthContextValue>({
  user: null,
  setUser: () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<IAuthUser | null>(() => readAuthUser());

  return <AuthContext.Provider value={{ user, setUser }}>{children}</AuthContext.Provider>;
}
