import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../../../contexts/JWTAuthContext";
import BrandMark from "./BrandMark";
import { roles } from "../../enums/roles";

const SITE = "https://www.dmbwebsolutions.com";

export default function Shell() {
  const { locations, locationId, setLocationId, logout, firstName, currentRole, isSuperAdmin } = useAuth();
  const [signingOut, setSigningOut] = useState(false);
  const isStaff = currentRole === roles.owner || currentRole === roles.admin || isSuperAdmin;
  const isTutor = currentRole === roles.tutor || isStaff;
  const isParent = currentRole === roles.parent || isStaff;

  const onSignOut = async () => {
    if (signingOut) return;
    setSigningOut(true);
    try { await logout(); } catch { setSigningOut(false); }
  };

  return (
    <div className="shell">
      <aside className="nav">
        <a className="brand-wrap" href={`${SITE}/ai-automation`}><BrandMark /></a>
        {firstName ? <div className="nav-greeting">Hi {firstName}</div> : null}
        <div className="location-switch">
          <label className="muted">Location</label>
          <select value={locationId ?? ""} onChange={(e) => setLocationId(e.target.value)}>
            {locations.map((l) => <option key={l.locationId} value={l.locationId}>{l.name}</option>)}
          </select>
        </div>
        <div className="nav-stack">
          <div className="nav-label">Workspace</div>
          <NavLink to="/" end>Dashboard</NavLink>
          {isParent ? <NavLink to="/children">Children</NavLink> : null}
          {isParent ? <NavLink to="/tutors">Find a tutor</NavLink> : null}
          {isParent ? <NavLink to="/schedule">Schedule</NavLink> : null}
          {isTutor ? <NavLink to="/profile">My profile</NavLink> : null}
          <NavLink to="/lessons">Lessons</NavLink>
          <NavLink to="/courses">Courses</NavLink>
          <NavLink to="/assignments">Assignments</NavLink>
          <NavLink to="/progress">Progress</NavLink>
          {isStaff ? <NavLink to="/admin">Manage users</NavLink> : null}
          <div className="nav-label">Your profile</div>
          <NavLink to="/account">Account</NavLink>
          <a href={`${SITE}/crm`}>CRM</a>
          <a href={`${SITE}/accent-sidebar/portfolio`}>Portfolio</a>
          <a href={SITE}>Website</a>
        </div>
        <button className="secondary" style={{ marginTop: "1.2rem", width: "100%" }} onClick={() => void onSignOut()} disabled={signingOut}>
          {signingOut ? "Signing out…" : "Sign out"}
        </button>
      </aside>
      <main className="main"><Outlet /></main>
    </div>
  );
}
