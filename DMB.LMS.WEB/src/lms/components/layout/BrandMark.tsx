const logoSrc = `${process.env.PUBLIC_URL || ""}/dmb-web-solutions-logo.png`;

export default function BrandMark({ compact = false }: { compact?: boolean }) {
  return (
    <span className="brand">
      <img src={logoSrc} alt="LMS" width={48} height={48} />
      <span className="brand-name">
        LMS
        {compact ? null : <span className="brand-sub">Learning Management System</span>}
      </span>
    </span>
  );
}
