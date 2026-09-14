export function firstNameFromToken(token: string | null) {
  if (!token) return "";
  try {
    const payload = JSON.parse(atob(token.split(".")[1] || ""));
    return String(payload.given_name || payload.unique_name || "").split("@")[0];
  } catch {
    return "";
  }
}
