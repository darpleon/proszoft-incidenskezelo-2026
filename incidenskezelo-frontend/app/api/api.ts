import axiosInstance from "./axios.config";
import type { IHealth } from "~/interfaces/IHealth";

const Health = {
  getHealth: () => axiosInstance.get<IHealth>(`/health`),
};

const api = { Health };

export default api;
