import { FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../../contexts/JWTAuthContext";
import BrandMark from "../layout/BrandMark";
import PasswordField from "./PasswordField";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
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
    <div className="auth-page">
      <form className="card" onSubmit={onSubmit}>
        <BrandMark />
        <h1>Sign in</h1>
        <p className="muted">Parent, tutor, or admin workspace.</p>
        {error ? <p className="error">{error}</p> : null}
        <div className="field"><label>Email</label><input value={username} onChange={(e) => setUsername(e.target.value)} required disabled={busy} /></div>
        <PasswordField label="Password" value={password} onChange={setPassword} disabled={busy} />
        <button type="submit" disabled={busy}>{busy ? "Signing in…" : "Sign in"}</button>
        <div className="auth-links">
          <Link to="/register">Create account</Link>
          <Link to="/forgot-password">Forgot password?</Link>
        </div>
      </form>
    </div>
  );
}
