import { FormEvent, useEffect, useState } from "react";
import http from "../../services/http.service";
import type { AuthProfile } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";
import PasswordField from "../auth/PasswordField";
import { apiErrorMessage } from "../../utils/apiError";

export default function AccountPage() {
  const { refreshProfile } = useAuth();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    contactNo: "",
    currentPassword: "",
    newPassword: "",
  });
  const [error, setError] = useState("");
  const [saved, setSaved] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    http.get<AuthProfile>("/auth/me").then((res) => {
      setForm((current) => ({
        ...current,
        firstName: res.data.firstName ?? "",
        lastName: res.data.lastName ?? "",
        email: res.data.email ?? "",
        contactNo: res.data.contactNo ?? "",
      }));
    }).catch(() => setError("Could not load your account."));
  }, []);

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError("");
    setSaved("");
    if (!form.firstName.trim() || !form.lastName.trim() || !form.email.trim()) {
      setError("First name, last name, and email are required.");
      return;
    }
    if (form.newPassword && !form.currentPassword) {
      setError("Enter your current password to set a new one.");
      return;
    }
    setBusy(true);
    try {
      await http.put("/auth/me", {
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        contactNo: form.contactNo.trim() || null,
        currentPassword: form.currentPassword || null,
        newPassword: form.newPassword || null,
      });
      setForm((current) => ({ ...current, currentPassword: "", newPassword: "" }));
      await refreshProfile();
      setSaved("Account updated.");
    } catch (err: unknown) {
      setError(apiErrorMessage(err, "Could not update your account."));
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <h1>Account</h1>
      <form className="card" onSubmit={(event) => void onSubmit(event)}>
        {error ? <p className="error">{error}</p> : null}
        {saved ? <p className="muted">{saved}</p> : null}
        <div className="field-row">
          <div className="field">
            <label>First name</label>
            <input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required disabled={busy} />
          </div>
          <div className="field">
            <label>Last name</label>
            <input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required disabled={busy} />
          </div>
        </div>
        <div className="field">
          <label>Email</label>
          <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required disabled={busy} />
        </div>
        <div className="field">
          <label>Phone</label>
          <input value={form.contactNo} onChange={(e) => setForm({ ...form, contactNo: e.target.value })} disabled={busy} />
        </div>
        <PasswordField
          label="Current password"
          value={form.currentPassword}
          onChange={(value) => setForm({ ...form, currentPassword: value })}
          autoComplete="current-password"
          required={false}
          disabled={busy}
        />
        <PasswordField
          label="New password"
          value={form.newPassword}
          onChange={(value) => setForm({ ...form, newPassword: value })}
          autoComplete="new-password"
          required={false}
          disabled={busy}
        />
        <p className="muted">Leave password fields blank to keep your current password.</p>
        <button type="submit" disabled={busy}>{busy ? "Saving…" : "Save account"}</button>
      </form>
    </div>
  );
}
