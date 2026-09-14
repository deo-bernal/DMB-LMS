import { useState } from "react";

export default function PasswordField({
  label, value, onChange, autoComplete = "current-password", required = true, disabled = false,
}: { label: string; value: string; onChange: (v: string) => void; autoComplete?: string; required?: boolean; disabled?: boolean }) {
  const [visible, setVisible] = useState(false);
  return (
    <div className="field">
      <label>{label}</label>
      <div className="password-field">
        <input type={visible ? "text" : "password"} value={value} onChange={(e) => onChange(e.target.value)} autoComplete={autoComplete} required={required} disabled={disabled} />
        <button type="button" className="password-toggle" onClick={() => setVisible((v) => !v)}>{visible ? "Hide" : "Show"}</button>
      </div>
    </div>
  );
}
