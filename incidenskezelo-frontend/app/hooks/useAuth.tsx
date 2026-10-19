import { isAxiosError } from "axios";
import { useContext } from "react";
import api from "~/api/api";
import { AuthContext } from "~/context/AuthContext";
import { clearToken, readAuthUser, saveToken } from "~/lib/authToken";

const loginErrors: Record<number, string> = {
  401: "The email address or password is incorrect.",
  403: "Your account is waiting for admin approval or has been deactivated.",
};

const useAuth = () => {
  const { user, setUser } = useContext(AuthContext);

  const login = async (email: string, password: string) => {
    try {
      const response = await api.User.login(email, password);
      saveToken(response.data.token);
      setUser(readAuthUser());
      return { success: true as const };
    } catch (error) {
      const status = isAxiosError(error) ? error.response?.status : undefined;
      const message = status === undefined ? undefined : loginErrors[status];
      return { success: false as const, error: message ?? "Sign-in is not available right now. Please try again later." };
    }
  };

  const logout = () => {
    clearToken();
    setUser(null);
  };

  return { user, isLoggedIn: user !== null, isAdmin: user?.role === "Admin", login, logout };
};

export default useAuth;
