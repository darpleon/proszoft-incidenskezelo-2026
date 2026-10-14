import axiosInstance from "./axios.config";
import type { IHealth } from "~/interfaces/IHealth";
import type { ILoginResponse, IUser, UserRole } from "~/interfaces/IUser";

const Health = {
  getHealth: () => axiosInstance.get<IHealth>(`/Health`),
};

const User = {
  login: (email: string, password: string) => axiosInstance.post<ILoginResponse>(`/users/login`, { email, password }),
  register: (name: string, email: string, password: string) => axiosInstance.post<IUser>(`/users/register`, { name, email, password }),
  getSetupStatus: () => axiosInstance.get<{ needsSetup: boolean }>(`/users/setup`),
  createFirstAdmin: (name: string, email: string, password: string) => axiosInstance.post<IUser>(`/users/setup`, { name, email, password }),
  getMe: () => axiosInstance.get<IUser>(`/users/me`),
  getUsers: () => axiosInstance.get<IUser[]>(`/users`),
  getRoles: () => axiosInstance.get<UserRole[]>(`/users/roles`),
  updateRole: (userId: number, role: UserRole) => axiosInstance.put<IUser>(`/users/${userId}/role`, { role }),
  setActive: (userId: number, isActive: boolean) => axiosInstance.put<IUser>(`/users/${userId}/active`, { isActive }),
};

const api = { Health, User };

export default api;
