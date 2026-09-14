import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import http from "../../services/http.service";
import type { AdminUser } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";
import { apiErrorMessage } from "../../utils/apiError";

const ROLES = ["owner", "admin", "tutor", "parent"];

type Draft = {
  firstName: string;
  lastName: string;
  email: string;
  contactNo: string;
  role: string;
  activated: boolean;
};

function displayName(user: AdminUser) {
  return [user.firstName, user.lastName].filter(Boolean).join(" ") || user.email;
}

function toDraft(user: AdminUser): Draft {
  return {
    firstName: user.firstName ?? "",
    lastName: user.lastName ?? "",
    email: user.email ?? "",
    contactNo: user.contactNo ?? "",
    role: user.role || "parent",
    activated: Boolean(user.activated),
  };
}

export default function ManageUsersPanel() {
  const { locationId, userId } = useAuth();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [query, setQuery] = useState("");
  const [error, setError] = useState("");
  const [busyUserId, setBusyUserId] = useState<string | null>(null);
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const [draft, setDraft] = useState<Draft | null>(null);
  const [deletingUser, setDeletingUser] = useState<AdminUser | null>(null);

  const load = useCallback(async () => {
    if (!locationId) return;
    setError("");
    try {
      const res = await http.get<AdminUser[]>("/admin/users");
      setUsers(res.data ?? []);
    } catch {
      setError("Could not load users.");
    }
  }, [locationId]);

  useEffect(() => { void load(); }, [load]);

  const visibleUsers = useMemo(() => {
    const needle = query.trim().toLowerCase();
    if (!needle) return users;
    return users.filter((user) =>
      [user.firstName, user.lastName, user.email, user.contactNo, user.role]
        .filter(Boolean)
        .join(" ")
        .toLowerCase()
        .includes(needle)
    );
  }, [query, users]);

  const openEdit = (user: AdminUser) => {
    setError("");
    setEditingUser(user);
    setDraft(toDraft(user));
  };

  const handleSave = async (e?: FormEvent) => {
    e?.preventDefault();
    if (!editingUser || !draft) return;
    const firstName = draft.firstName.trim();
    const lastName = draft.lastName.trim();
    const email = draft.email.trim();
    if (!firstName || !lastName || !email) {
      setError("First name, last name, and email are required.");
      return;
    }
    setBusyUserId(editingUser.userId);
    setError("");
    try {
      const payload = {
        firstName,
        lastName,
        email,
        contactNo: draft.contactNo.trim() || null,
        role: draft.role,
        activated: draft.activated,
      };
      const res = await http.put<AdminUser>(`/admin/users/${editingUser.userId}`, payload);
      setUsers((current) => current.map((item) => (item.userId === editingUser.userId ? { ...item, ...res.data } : item)));
      setEditingUser(null);
      setDraft(null);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not update that user."));
    } finally {
      setBusyUserId(null);
    }
  };

  const handleDelete = async () => {
    if (!deletingUser) return;
    setBusyUserId(deletingUser.userId);
    setError("");
    try {
      const res = await http.delete<{ message?: string; deactivated?: boolean }>(`/admin/users/${deletingUser.userId}`);
      if (res.data?.deactivated) {
        setUsers((current) => current.map((item) => (item.userId === deletingUser.userId ? { ...item, activated: false } : item)));
      } else {
        setUsers((current) => current.filter((item) => item.userId !== deletingUser.userId));
      }
      setDeletingUser(null);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not delete that user."));
    } finally {
      setBusyUserId(null);
    }
  };

  return (
    <div className="card">
      <h2>Manage users</h2>
      {error ? <p className="error">{error}</p> : null}
      <div className="field">
        <label>Search users</label>
        <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Name, email, phone, or role" />
      </div>
      {visibleUsers.length === 0 ? (
        <p className="muted">No users match that search.</p>
      ) : (
        visibleUsers.map((user) => {
          const isSelf = userId === user.userId;
          const canDelete = !user.isSuperAdmin && !isSelf;
          return (
            <div className="user-row" key={user.userId}>
              <div>
                <strong>{displayName(user)}</strong>
                <div className="muted">
                  {user.email}
                  {user.contactNo ? ` · ${user.contactNo}` : ""}
                  {` · ${user.role}`}
                  {user.isSuperAdmin ? " · super admin" : ""}
                  {user.activated ? "" : " · not activated"}
                </div>
              </div>
              <div className="user-actions">
                <button className="ghost" type="button" disabled={busyUserId === user.userId} onClick={() => openEdit(user)}>Edit</button>
                <button className="ghost" type="button" disabled={!canDelete || busyUserId === user.userId} onClick={() => { setError(""); setDeletingUser(user); }}>Delete</button>
              </div>
            </div>
          );
        })
      )}

      {editingUser && draft ? (
        <div className="dialog-backdrop" onClick={() => { if (!busyUserId) { setEditingUser(null); setDraft(null); } }}>
          <form className="card dialog-card" onClick={(e) => e.stopPropagation()} onSubmit={(e) => void handleSave(e)}>
            <h2>Update user</h2>
            <div className="field"><label>First name</label><input value={draft.firstName} onChange={(e) => setDraft({ ...draft, firstName: e.target.value })} required /></div>
            <div className="field"><label>Last name</label><input value={draft.lastName} onChange={(e) => setDraft({ ...draft, lastName: e.target.value })} required /></div>
            <div className="field"><label>Email</label><input type="email" value={draft.email} onChange={(e) => setDraft({ ...draft, email: e.target.value })} required /></div>
            <div className="field"><label>Phone</label><input value={draft.contactNo} onChange={(e) => setDraft({ ...draft, contactNo: e.target.value })} /></div>
            <div className="field">
              <label>Role</label>
              <select value={draft.role} onChange={(e) => setDraft({ ...draft, role: e.target.value })} disabled={Boolean(editingUser.isSuperAdmin)}>
                {ROLES.map((role) => <option key={role} value={role}>{role}</option>)}
              </select>
            </div>
            <label className="chip" style={{ marginBottom: "0.8rem" }}>
              <input type="checkbox" checked={draft.activated} onChange={(e) => setDraft({ ...draft, activated: e.target.checked })} /> Activated
            </label>
            <div className="user-actions">
              <button className="ghost" type="button" disabled={Boolean(busyUserId)} onClick={() => { setEditingUser(null); setDraft(null); }}>Cancel</button>
              <button type="submit" disabled={Boolean(busyUserId)}>{busyUserId ? "Saving…" : "Save changes"}</button>
            </div>
          </form>
        </div>
      ) : null}

      {deletingUser ? (
        <div className="dialog-backdrop" onClick={() => { if (!busyUserId) setDeletingUser(null); }}>
          <div className="card dialog-card" onClick={(e) => e.stopPropagation()}>
            <h2>Delete user</h2>
            <p>Permanently delete {displayName(deletingUser)} ({deletingUser.email})? If they still have students or lessons, the account will be deactivated instead.</p>
            <div className="user-actions">
              <button className="ghost" type="button" disabled={Boolean(busyUserId)} onClick={() => setDeletingUser(null)}>Cancel</button>
              <button type="button" disabled={Boolean(busyUserId)} onClick={() => void handleDelete()}>{busyUserId ? "Deleting…" : "Delete user"}</button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
