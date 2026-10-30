import axiosInstance from "./axios.config";
import { aiRequestTimeoutMs } from "~/constants/constants";
import type { AiModel, IAiAnalysis } from "~/interfaces/IAiAnalysis";
import type { IEvent } from "~/interfaces/IEvent";
import type { IHealth } from "~/interfaces/IHealth";
import type { ICreateIncidentRequest, IIncident, IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { IAssignableUser, ILoginResponse, IUser, UserRole } from "~/interfaces/IUser";

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
  getAssignable: () => axiosInstance.get<IAssignableUser[]>(`/users/assignable`),
  getRoles: () => axiosInstance.get<UserRole[]>(`/users/roles`),
  updateRole: (userId: number, role: UserRole) => axiosInstance.put<IUser>(`/users/${userId}/role`, { role }),
  setActive: (userId: number, isActive: boolean) => axiosInstance.put<IUser>(`/users/${userId}/active`, { isActive }),
};

const Incident = {
  getAll: () => axiosInstance.get<IIncident[]>(`/incidents`),
  getById: (id: string) => axiosInstance.get<IIncident>(`/incidents/${id}`),
  create: (request: ICreateIncidentRequest) => axiosInstance.post<IIncident>(`/incidents`, request),
  getEvents: (id: string) => axiosInstance.get<IEvent[]>(`/incidents/${id}/events`),
  updateStatus: (id: string, status: IncidentStatus, expectedRevision: number) =>
    axiosInstance.patch<IIncident>(`/incidents/${id}/status`, { status, expectedRevision }),
  updatePriority: (id: string, priority: IncidentPriority, expectedRevision: number) =>
    axiosInstance.patch<IIncident>(`/incidents/${id}/priority`, { priority, expectedRevision }),
  updateAssignee: (id: string, assigneeUserId: number | null, expectedRevision: number) =>
    axiosInstance.patch<IIncident>(`/incidents/${id}/assignee`, { assigneeUserId, expectedRevision }),
};

const Event = {
  getLatest: () => axiosInstance.get<IEvent[]>(`/events`),
};

const Ai = {
  analyze: (incidentId: string, model: AiModel) =>
    axiosInstance.post<IAiAnalysis>(`/ai/analyze/${incidentId}`, { model }, { timeout: aiRequestTimeoutMs }),
};

const api = { Health, User, Incident, Event, Ai };

export default api;
