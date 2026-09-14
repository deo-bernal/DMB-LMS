import { FormEvent, useState } from "react";
import { Link } from "react-router-dom";
import http from "../../services/http.service";
import AuthLayout from "./AuthLayout";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const res = await http.post("/auth/forgot-password", { email });
    setMessage(res.data.message);
  };
  return (
    <AuthLayout>
      <form className="card" onSubmit={onSubmit}>
        <h1>Forgot password</h1>
        {message ? <p>{message}</p> : null}
        <div className="field"><label>Email</label><input value={email} onChange={(e) => setEmail(e.target.value)} required /></div>
        <button type="submit">Send reset link</button>
        <div className="auth-links"><Link to="/login">Back to sign in</Link></div>
      </form>
    </AuthLayout>
  );
}
