import { useState } from "react";

type PasswordFieldProps = {
  label: string;
  value: string;
  onChange: (value: string) => void;
  autoComplete?: string;
  required?: boolean;
  disabled?: boolean;
};

export default function PasswordField({
  label,
  value,
  onChange,
  autoComplete = "current-password",
  required = true,
  disabled = false,
}: PasswordFieldProps) {
  const [visible, setVisible] = useState(false);

  return (
    <div className="field">
      <label>{label}</label>
      <div className="password-field">
        <input
          type={visible ? "text" : "password"}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          autoComplete={autoComplete}
          required={required}
          disabled={disabled}
        />
        <button
          type="button"
          className="password-toggle"
          onClick={() => setVisible((prev) => !prev)}
          disabled={disabled}
          aria-label={visible ? "Hide password" : "Show password"}
          title={visible ? "Hide password" : "Show password"}
        >
          {visible ? (
            <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden>
              <path
                fill="currentColor"
                d="M12 5c-7 0-10 7-10 7s3 7 10 7 10-7 10-7-3-7-10-7zm0 12a5 5 0 1 1 0-10 5 5 0 0 1 0 10zm0-8a3 3 0 1 0 0 6 3 3 0 0 0 0-6z"
              />
            </svg>
          ) : (
            <svg viewBox="0 0 24 24" width="18" height="18" aria-hidden>
              <path
                fill="currentColor"
                d="M2.1 3.51 3.51 2.1 21.9 20.49 20.49 21.9l-3.2-3.2A12.7 12.7 0 0 1 12 19C5 19 2 12 2 12a14.8 14.8 0 0 1 5.3-5.7L2.1 3.51zM12 7a5 5 0 0 1 4.9 6.1l-1.6-1.6A3 3 0 0 0 12 9c-.3 0-.6 0-.9.1L9.5 7.5A5 5 0 0 1 12 7zm9.3 5.7A14.7 14.7 0 0 0 12 5c-.7 0-1.3.1-2 .2L8.3 3.5C9.5 3.2 10.7 3 12 3c7 0 10 7 10 7a16 16 0 0 1-3.4 4.6l-1.5-1.5c.9-.7 1.6-1.5 2.2-2.4z"
              />
            </svg>
          )}
        </button>
      </div>
    </div>
  );
}
