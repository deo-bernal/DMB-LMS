function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split(".")[1];
    if (!part) return null;
    const padded = part.replace(/-/g, "+").replace(/_/g, "/");
    const json = atob(padded);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function claimString(payload: Record<string, unknown>, keys: string[]): string {
  for (const key of keys) {
    const value = payload[key];
    if (typeof value === "string" && value.trim()) return value.trim();
  }
  return "";
}

export function firstNameFromToken(token: string | null) {
  if (!token) return "";
  const payload = decodeJwtPayload(token);
  if (!payload) return "";
  const name = claimString(payload, [
    "given_name",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
    "unique_name",
  ]);
  return name.split("@")[0];
}

export function userIdFromToken(token: string | null) {
  if (!token) return "";
  const payload = decodeJwtPayload(token);
  if (!payload) return "";
  return claimString(payload, [
    "nameid",
    "sub",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  ]);
}

export function isSuperAdminFromToken(token: string | null) {
  if (!token) return false;
  const payload = decodeJwtPayload(token);
  if (!payload) return false;
  const value = payload.isSuperAdmin;
  return value === true || value === "true";
}
