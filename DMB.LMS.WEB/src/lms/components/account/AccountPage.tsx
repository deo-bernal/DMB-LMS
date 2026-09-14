import { FormEvent, useEffect, useState } from "react";
import http from "../../services/http.service";
import type { AuthProfile } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";
import { apiErrorMessage } from "../../utils/apiError";
import PasswordField from "../auth/PasswordField";

export default function AccountPage() {
  const { refreshProfile } = useAuth();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [contactNo, setContactNo] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState("");
  const [saved, setSaved] = useState("");
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    void http.get<AuthProfile>("/auth/me").then((res) => {
      setFirstName(res.data.firstName ?? "");
      setLastName(res.data.lastName ?? "");
      setEmail(res.data.email ?? "");
      setContactNo(res.data.contactNo ?? "");
    }).catch(() => setError("Could not load your account."));
  }, []);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setSaved("");
    if (!firstName.trim() || !lastName.trim() || !email.trim()) {
      setError("First name, last name, and email are required.");
      return;
    }
    if (newPassword && !currentPassword) {
      setError("Enter your current password to set a new one.");
      return;
    }
    setBusy(true);
    try {
      await http.put("/auth/me", {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        contactNo: contactNo.trim() || null,
        currentPassword: currentPassword || null,
        newPassword: newPassword || null,
      });
      setCurrentPassword("");
      setNewPassword("");
      setSaved("Account updated.");
      await refreshProfile();
    } catch (err) {
      setError(apiErrorMessage(err, "Could not update your account."));
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <h1>Account</h1>
      <form className="card" onSubmit={(e) => void onSubmit(e)} style={{ maxWidth: 520 }}>
        {error ? <p className="error">{error}</p> : null}
        {saved ? <p className="muted">{saved}</p> : null}
        <div className="field"><label>First name</label><input value={firstName} onChange={(e) => setFirstName(e.target.value)} required disabled={busy} /></div>
        <div className="field"><label>Last name</label><input value={lastName} onChange={(e) => setLastName(e.target.value)} required disabled={busy} /></div>
        <div className="field"><label>Email</label><input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required disabled={busy} /></div>
        <div className="field"><label>Phone</label><input value={contactNo} onChange={(e) => setContactNo(e.target.value)} disabled={busy} /></div>
        <PasswordField label="Current password (to change password)" value={currentPassword} onChange={setCurrentPassword} required={false} autoComplete="current-password" disabled={busy} />
        <PasswordField label="New password" value={newPassword} onChange={setNewPassword} required={false} autoComplete="new-password" disabled={busy} />
        <button type="submit" disabled={busy}>{busy ? "Saving…" : "Save changes"}</button>
      </form>
    </div>
  );
}
