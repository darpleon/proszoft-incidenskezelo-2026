import axios from "axios";
import { clearToken, getToken } from "~/lib/authToken";

const baseURL = import.meta.env.VITE_REST_API_URL || "/api";

const axiosInstance = axios.create({
  baseURL,
});

axiosInstance.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    const isLoginRequest = error.config?.url?.endsWith("/users/login");
    if (error.response?.status === 401 && !isLoginRequest) {
      clearToken();
      window.location.assign("/login");
    }
    return Promise.reject(error);
  },
);

export default axiosInstance;
