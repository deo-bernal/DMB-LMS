const logoSrc = `${process.env.PUBLIC_URL || ""}/dmb-web-solutions-logo.png`;

export default function BrandMark({ compact = false }: { compact?: boolean }) {
  return (
    <span className="brand">
      <img src={logoSrc} alt="DMB Web Solutions" />
      <span className="brand-name">
        DMB Web Solutions
        {compact ? null : <span className="brand-sub">LMS</span>}
      </span>
    </span>
  );
}
