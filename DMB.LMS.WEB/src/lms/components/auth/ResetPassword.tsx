import { FormEvent, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import http from "../../services/http.service";
import BrandMark from "../layout/BrandMark";
import PasswordField from "./PasswordField";

export default function ResetPassword() {
  const [params] = useSearchParams();
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirm] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    try {
      const res = await http.post("/auth/reset-password", { token: params.get("token"), password, confirmPassword });
      setMessage(res.data.message);
    } catch (err: any) {
      setError(err?.response?.data?.message || "Reset failed.");
    }
  };
  return (
    <div className="auth-page">
      <form className="card" onSubmit={onSubmit}>
        <BrandMark /><h1>Reset password</h1>
        {message ? <p>{message}</p> : null}
        {error ? <p className="error">{error}</p> : null}
        <PasswordField label="New password" value={password} onChange={setPassword} autoComplete="new-password" />
        <PasswordField label="Confirm" value={confirmPassword} onChange={setConfirm} autoComplete="new-password" />
        <button type="submit">Save password</button>
        <div className="auth-links"><Link to="/login">Back to sign in</Link></div>
      </form>
    </div>
  );
}
