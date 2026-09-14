export default function SiteFooter() {
  const year = new Date().getFullYear();
  return (
    <footer className="site-footer">
      © {year} DMB Web Solutions ·{" "}
      <a href="https://www.dmbwebsolutions.com/">dmbwebsolutions.com</a>
      {" · "}
      <a href="https://www.dmbwebsolutions.com/profiles#lots">Lots in Pampanga</a>
    </footer>
  );
}
