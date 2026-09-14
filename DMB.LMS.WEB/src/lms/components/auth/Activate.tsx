import { FormEvent, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import http from "../../services/http.service";
import BrandMark from "../layout/BrandMark";

export default function Activate() {
  const [params] = useSearchParams();
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    try {
      const res = await http.post("/registration/activate", { token: params.get("token") });
      setMessage(res.data.message);
    } catch (err: any) {
      setError(err?.response?.data?.message || "Activation failed.");
    }
  };
  return (
    <div className="auth-page">
      <form className="card" onSubmit={onSubmit}>
        <BrandMark /><h1>Activate account</h1>
        {message ? <p>{message}</p> : null}
        {error ? <p className="error">{error}</p> : null}
        <button type="submit">Activate</button>
        <div className="auth-links"><Link to="/login">Back to sign in</Link></div>
      </form>
    </div>
  );
}
