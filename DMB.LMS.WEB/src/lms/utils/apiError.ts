export function apiErrorMessage(error: unknown, fallback: string) {
  const data = (error as { response?: { data?: { message?: string } } })?.response?.data;
  if (data && typeof data.message === "string" && data.message.trim()) {
    return data.message;
  }
  return fallback;
}
