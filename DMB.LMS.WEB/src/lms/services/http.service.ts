import axios from "axios";
import { lmsApiConfig } from "../../config";
import { beginLoading, endLoading, tracksPageLoading } from "../utils/loadingGate";

const http = axios.create({
  baseURL: lmsApiConfig.lms_api_url,
  headers: { "Content-type": "application/json" },
});

http.interceptors.request.use((config) => {
  const token = localStorage.getItem("lms_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  const locationId = localStorage.getItem("lms_locationId");
  if (locationId) config.headers["X-Location-Id"] = locationId;
  if (tracksPageLoading(config.url)) beginLoading();
  return config;
});

http.interceptors.response.use(
  (response) => {
    if (tracksPageLoading(response.config.url)) endLoading();
    return response;
  },
  (error) => {
    const requestUrl = String(error?.config?.url ?? "");
    if (tracksPageLoading(requestUrl)) endLoading();
    const status = error?.response?.status;
    if (status === 401 && !/\/auth\/(login|logout|external|me)/i.test(requestUrl) && localStorage.getItem("lms_token")) {
      localStorage.removeItem("lms_token");
      window.dispatchEvent(new Event("lms:unauthorized"));
    }
    return Promise.reject(error);
  }
);

export default http;
