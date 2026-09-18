export const lmsApiConfig = {
  lms_api_url: process.env.REACT_APP_LMS_API_URL || "http://localhost:5089/api",
};

/** Prefer same-origin /lms/api on the live site to avoid CORS preflight to Render. */
export function resolveApiBaseUrl() {
  if (typeof window !== "undefined" && /(?:^|\.)dmbwebsolutions\.com$/i.test(window.location.hostname)) {
    return `${window.location.origin}/lms/api`;
  }
  return lmsApiConfig.lms_api_url;
}

export function warmupApi() {
  if (typeof window === "undefined") return;
  const health = `${resolveApiBaseUrl().replace(/\/$/, "")}/health`;
  void fetch(health, { method: "GET", cache: "no-store", credentials: "omit" }).catch(() => undefined);
}
