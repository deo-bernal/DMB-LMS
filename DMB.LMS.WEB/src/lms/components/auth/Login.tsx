import { FormEvent, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../../contexts/JWTAuthContext";
import AuthLayout from "./AuthLayout";
import PasswordField from "./PasswordField";
import SocialAuthButtons from "./SocialAuthButtons";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(params.get("ssoError")?.trim() ?? "");
  const [busy, setBusy] = useState(false);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setBusy(true);
    try {
      await login(username, password);
      navigate("/", { replace: true });
    } catch (err: any) {
      setError(err?.response?.data?.message || "Invalid credentials.");
      setBusy(false);
    }
  };

  return (
    <AuthLayout>
      <form className="card" onSubmit={onSubmit}>
        <h1>Sign in</h1>
        <p className="muted">Parent, tutor, or admin workspace.</p>
        <SocialAuthButtons />
        {error ? <p className="error">{error}</p> : null}
        <div className="field"><label>Email</label><input value={username} onChange={(e) => setUsername(e.target.value)} required disabled={busy} /></div>
        <PasswordField label="Password" value={password} onChange={setPassword} disabled={busy} />
        <button type="submit" disabled={busy}>{busy ? "Signing in…" : "Sign in"}</button>
        <div className="auth-links">
          <Link to="/register">Create account</Link>
          <Link to="/forgot-password">Forgot password?</Link>
        </div>
      </form>
    </AuthLayout>
  );
}
