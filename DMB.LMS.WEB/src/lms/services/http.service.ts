import axios, { type AxiosRequestConfig } from "axios";
import { lmsApiConfig } from "../../config";
import { beginLoading, endLoading, tracksPageLoading } from "../utils/loadingGate";

type RequestConfig = AxiosRequestConfig & { skipLoading?: boolean };

const http = axios.create({
  baseURL: lmsApiConfig.lms_api_url,
  headers: { "Content-type": "application/json" },
});

function tracked(config?: RequestConfig) {
  return tracksPageLoading(config?.url, config?.skipLoading);
}

http.interceptors.request.use((config) => {
  const token = localStorage.getItem("lms_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  const locationId = localStorage.getItem("lms_locationId");
  if (locationId) config.headers["X-Location-Id"] = locationId;
  if (tracked(config)) beginLoading();
  return config;
});

http.interceptors.response.use(
  (response) => {
    if (tracked(response.config)) endLoading();
    return response;
  },
  (error) => {
    const requestUrl = String(error?.config?.url ?? "");
    if (tracked(error?.config)) endLoading();
    const status = error?.response?.status;
    if (status === 401 && !/\/auth\/(login|logout|external|me)/i.test(requestUrl) && localStorage.getItem("lms_token")) {
      localStorage.removeItem("lms_token");
      window.dispatchEvent(new Event("lms:unauthorized"));
    }
    return Promise.reject(error);
  }
);

export default http;
