import { useEffect, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../../contexts/JWTAuthContext";
import AuthLayout from "./AuthLayout";

export default function AuthCallback() {
  const { acceptSession } = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get("token")?.trim() ?? "";
  const redirect = params.get("redirect")?.trim() || "/";
  const [error, setError] = useState("");

  useEffect(() => {
    if (!token) {
      setError("Sign-in did not return a session. Try again from the login page.");
      return;
    }

    let cancelled = false;
    (async () => {
      try {
        await acceptSession(token);
        if (!cancelled) {
          navigate(redirect.startsWith("/") ? redirect : "/", { replace: true });
        }
      } catch {
        if (!cancelled) {
          setError("Could not finish social sign-in. Try again.");
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [token, acceptSession, navigate, redirect]);

  return (
    <AuthLayout>
      <div className="card">
        <h1>Signing you in</h1>
        <p className="muted">Finishing social sign-in.</p>
        {error ? (
          <>
            <p className="error">{error}</p>
            <div className="auth-links">
              <Link to="/login">Back to sign in</Link>
            </div>
          </>
        ) : (
          <p className="muted">Please wait…</p>
        )}
      </div>
    </AuthLayout>
  );
}
