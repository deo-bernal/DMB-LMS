const PROVIDERS = [
  { id: "google", label: "Continue with Google" },
  { id: "linkedin", label: "Continue with LinkedIn" },
  { id: "facebook", label: "Continue with Facebook" },
] as const;

function ProviderIcon({ provider }: { provider: (typeof PROVIDERS)[number]["id"] }) {
  if (provider === "google") {
    return (
      <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden>
        <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
        <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
        <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" />
        <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" />
      </svg>
    );
  }

  if (provider === "linkedin") {
    return (
      <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden>
        <path fill="#0A66C2" d="M20.45 20.45h-3.55v-5.57c0-1.33-.03-3.04-1.85-3.04-1.85 0-2.14 1.45-2.14 2.94v5.67H9.35V9h3.41v1.56h.05c.47-.9 1.64-1.85 3.37-1.85 3.6 0 4.27 2.37 4.27 5.46v6.28zM5.34 7.43a2.06 2.06 0 1 1 0-4.12 2.06 2.06 0 0 1 0 4.12zM7.12 20.45H3.56V9h3.56v11.45z" />
      </svg>
    );
  }

  return (
    <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden>
      <path fill="#1877F2" d="M24 12.07C24 5.4 18.63 0 12 0S0 5.4 0 12.07C0 18.1 4.39 23.09 10.13 24v-8.44H7.08v-3.49h3.05V9.41c0-3.02 1.79-4.7 4.54-4.7 1.31 0 2.69.24 2.69.24v2.97h-1.52c-1.5 0-1.96.93-1.96 1.89v2.26h3.34l-.53 3.49h-2.81V24C19.61 23.09 24 18.1 24 12.07z" />
    </svg>
  );
}

function startHref(provider: string, role?: string) {
  const params = new URLSearchParams({ client: "web" });
  if (role === "tutor" || role === "parent") {
    params.set("role", role);
  }

  if (typeof window !== "undefined" && /(?:^|\.)dmbwebsolutions\.com$/i.test(window.location.hostname)) {
    return `https://www.dmbwebsolutions.com/lms/api/auth/external/${provider}/start?${params}`;
  }

  const apiBase = (process.env.REACT_APP_LMS_API_URL || "http://localhost:5089/api").replace(/\/$/, "");
  return `${apiBase}/auth/external/${provider}/start?${params}`;
}

export default function SocialAuthButtons({ role }: { role?: string }) {
  return (
    <div className="sso-stack">
      {PROVIDERS.map((provider) => (
        <a key={provider.id} className="sso-button" href={startHref(provider.id, role)}>
          <ProviderIcon provider={provider.id} />
          {provider.label}
        </a>
      ))}
      <div className="sso-divider">
        <span>or</span>
      </div>
    </div>
  );
}
