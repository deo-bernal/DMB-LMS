import { FormEvent, useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../../contexts/JWTAuthContext";
import http from "../../services/http.service";
import AuthLayout from "./AuthLayout";

export default function AuthComplete() {
  const { acceptSession } = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const ticket = useMemo(() => params.get("ticket")?.trim() ?? "", [params]);
  const [step, setStep] = useState<"email" | "code">("email");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [code, setCode] = useState("");
  const [error, setError] = useState("");

  const onSendCode = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    try {
      await http.post("/auth/external/complete", { ticket, email: email.trim(), phone: phone.trim() || null });
      setStep("code");
    } catch (err: any) {
      setError(err?.response?.data?.message || "Could not send the verification code.");
    }
  };

  const onVerify = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    try {
      const res = await http.post<{ token: string; locations?: any[]; currentLocationId?: string; firstName?: string }>(
        "/auth/external/verify",
        { ticket, code: code.trim() }
      );
      if (!res.data?.token) {
        setError("Sign-in did not return a session. Try again.");
        return;
      }
      await acceptSession(res.data.token, res.data.locations, res.data.currentLocationId, res.data.firstName);
      navigate("/", { replace: true });
    } catch (err: any) {
      setError(err?.response?.data?.message || "That code could not be verified.");
    }
  };

  return (
    <AuthLayout>
      <form className="card" onSubmit={step === "email" ? onSendCode : onVerify}>
        <h1>Finish signing up</h1>
        <p className="muted">
          {step === "email"
            ? "This social account did not share an email. Enter yours to finish."
            : "Enter the 6-digit code we emailed you."}
        </p>
        {!ticket ? (
          <p className="error">This sign-up session is missing. Start again with Google, LinkedIn, or Facebook.</p>
        ) : step === "email" ? (
          <>
            {error ? <p className="error">{error}</p> : null}
            <div className="field">
              <label>Email</label>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            <div className="field">
              <label>Phone (optional)</label>
              <input value={phone} onChange={(e) => setPhone(e.target.value)} />
            </div>
            <button type="submit">Send code</button>
          </>
        ) : (
          <>
            {error ? <p className="error">{error}</p> : null}
            <div className="field">
              <label>6-digit code</label>
              <input value={code} onChange={(e) => setCode(e.target.value)} maxLength={6} required />
            </div>
            <button type="submit">Verify and continue</button>
          </>
        )}
        <div className="auth-links">
          <Link to="/login">Back to sign in</Link>
        </div>
      </form>
    </AuthLayout>
  );
}
