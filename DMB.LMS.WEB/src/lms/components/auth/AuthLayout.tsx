import { ReactNode } from "react";
import BrandMark from "../layout/BrandMark";

export default function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="auth-page">
      <header className="auth-hero">
        <BrandMark />
      </header>
      {children}
    </div>
  );
}
