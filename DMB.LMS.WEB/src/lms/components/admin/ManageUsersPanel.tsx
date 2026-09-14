import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import http from "../../services/http.service";
import type { AdminUser } from "../../models";
import { useAuth } from "../../../contexts/JWTAuthContext";
import { apiErrorMessage } from "../../utils/apiError";

const DEFAULT_ROLES = [
  { value: "owner", label: "Owner" },
  { value: "admin", label: "Admin" },
  { value: "tutor", label: "Tutor" },
  { value: "parent", label: "Parent" },
];

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

function providerLabel(provider: string) {
  const value = provider.trim().toLowerCase();
  if (value === "google") return "Google";
  if (value === "linkedin") return "LinkedIn";
  if (value === "facebook") return "Facebook";
  return provider;
}

function signInSummary(user: AdminUser) {
  const apps = (user.linkedProviders ?? []).map(providerLabel).filter(Boolean);
  const methods = user.passwordSet === false ? [...apps] : ["Email and password", ...apps];
  return methods.join(" · ");
}

function toDraft(user: AdminUser, fallbackRole: string): Draft {
  return {
    firstName: user.firstName ?? "",
    lastName: user.lastName ?? "",
    email: user.email ?? "",
    contactNo: user.contactNo ?? "",
    role: user.role || fallbackRole,
    activated: Boolean(user.activated),
  };
}

export default function ManageUsersPanel({ roles = DEFAULT_ROLES }: { roles?: { value: string; label: string }[] }) {
  const { locationId, userId: currentUserId } = useAuth();
  const fallbackRole = roles[roles.length - 1]?.value ?? "";
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [query, setQuery] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [busyUserId, setBusyUserId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const [draft, setDraft] = useState<Draft | null>(null);
  const [deletingUser, setDeletingUser] = useState<AdminUser | null>(null);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const res = await http.get<AdminUser[]>("/admin/users");
      setUsers(res.data ?? []);
    } catch {
      setError("Could not load users. Owner or admin access is required.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!locationId) return;
    void load();
  }, [locationId, load]);

  useEffect(() => {
    if (!editingUser && !deletingUser) return;
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !busyUserId) {
        setEditingUser(null);
        setDraft(null);
        setDeletingUser(null);
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [editingUser, deletingUser, busyUserId]);

  const visibleUsers = useMemo(() => {
    const needle = query.trim().toLowerCase();
    if (!needle) return users;
    return users.filter((user) => {
      const summary = signInSummary(user);
      const haystack = [
        user.firstName,
        user.lastName,
        user.email,
        user.contactNo,
        user.role,
        user.isSuperAdmin ? "super admin" : "",
        user.activated ? "activated" : "not activated",
        summary,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();
      return haystack.includes(needle);
    });
  }, [query, users]);

  const openEdit = (user: AdminUser) => {
    setError(null);
    setEditingUser(user);
    setDraft(toDraft(user, fallbackRole));
  };

  const handleSave = async (event: FormEvent) => {
    event.preventDefault();
    if (!editingUser || !draft) return;
    const firstName = draft.firstName.trim();
    const lastName = draft.lastName.trim();
    const email = draft.email.trim();
    if (!firstName || !lastName || !email) {
      setError("First name, last name, and email are required.");
      return;
    }

    setBusyUserId(editingUser.userId);
    setError(null);
    try {
      const payload = { ...draft, firstName, lastName, email };
      const res = await http.put<AdminUser>(`/admin/users/${editingUser.userId}`, payload);
      setUsers((current) =>
        current.map((item) => (item.userId === editingUser.userId ? { ...item, ...res.data } : item))
      );
      setEditingUser(null);
      setDraft(null);
    } catch (err: unknown) {
      setError(apiErrorMessage(err, "Could not update that user."));
    } finally {
      setBusyUserId(null);
    }
  };

  const handleDelete = async () => {
    if (!deletingUser) return;
    setBusyUserId(deletingUser.userId);
    setError(null);
    try {
      const res = await http.delete<{ message?: string; deactivated?: boolean }>(`/admin/users/${deletingUser.userId}`);
      if (res.data?.deactivated) {
        setUsers((current) =>
          current.map((item) => (item.userId === deletingUser.userId ? { ...item, activated: false } : item))
        );
        setError(res.data.message ?? "This account was deactivated instead of deleted.");
      } else {
        setUsers((current) => current.filter((item) => item.userId !== deletingUser.userId));
      }
      setDeletingUser(null);
    } catch (err: unknown) {
      setError(apiErrorMessage(err, "Could not delete that user."));
    } finally {
      setBusyUserId(null);
    }
  };

  return (
    <>
      {error ? <p className="error">{error}</p> : null}
      <div className="field">
        <label htmlFor="user-search">Search users</label>
        <input
          id="user-search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Name, email, phone, role"
        />
      </div>

      {isLoading ? (
        <p className="muted">Loading users…</p>
      ) : visibleUsers.length === 0 ? (
        <p className="muted">No users match that search.</p>
      ) : (
        <div className="user-list">
          {visibleUsers.map((user) => {
            const isSelf = currentUserId === user.userId;
            const canDelete = !user.isSuperAdmin && !isSelf;
            const summary = signInSummary(user);
            return (
              <div className="user-row" key={user.userId}>
                <div>
                  <strong>{displayName(user)}</strong>
                  <div className="muted">
                    {user.email}
                    {user.contactNo ? ` · ${user.contactNo}` : ""}
                    {user.isSuperAdmin ? " · super admin" : ` · ${user.role}`}
                    {user.activated ? "" : " · not activated"}
                    {` · ${summary}`}
                  </div>
                </div>
                <div className="user-actions">
                  <button type="button" className="ghost" disabled={busyUserId === user.userId} onClick={() => openEdit(user)}>
                    Edit
                  </button>
                  <button
                    type="button"
                    className="ghost"
                    disabled={!canDelete || busyUserId === user.userId}
                    onClick={() => {
                      setError(null);
                      setDeletingUser(user);
                    }}
                  >
                    Delete
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {editingUser && draft ? (
        <div
          className="dialog-backdrop"
          onClick={() => {
            if (!busyUserId) {
              setEditingUser(null);
              setDraft(null);
            }
          }}
        >
          <form
            className="card dialog-card"
            role="dialog"
            aria-modal="true"
            aria-labelledby="update-user-title"
            onClick={(event) => event.stopPropagation()}
            onSubmit={(event) => void handleSave(event)}
          >
            <h2 id="update-user-title">Update user</h2>
            <div className="field-row">
              <div className="field">
                <label>First name</label>
                <input value={draft.firstName} onChange={(event) => setDraft({ ...draft, firstName: event.target.value })} required />
              </div>
              <div className="field">
                <label>Last name</label>
                <input value={draft.lastName} onChange={(event) => setDraft({ ...draft, lastName: event.target.value })} required />
              </div>
            </div>
            <div className="field">
              <label>Email</label>
              <input type="email" value={draft.email} onChange={(event) => setDraft({ ...draft, email: event.target.value })} required />
            </div>
            <div className="field">
              <label>Phone</label>
              <input value={draft.contactNo} onChange={(event) => setDraft({ ...draft, contactNo: event.target.value })} />
            </div>
            <div className="field">
              <label>Sign-in methods</label>
              <p className="muted" style={{ margin: 0 }}>{signInSummary(editingUser)}</p>
            </div>
            <div className="field">
              <label>Role</label>
              <select value={draft.role} onChange={(event) => setDraft({ ...draft, role: event.target.value })}>
                {roles.map((role) => (
                  <option key={role.value} value={role.value}>{role.label}</option>
                ))}
              </select>
            </div>
            <label className="switch-field">
              <input
                type="checkbox"
                checked={draft.activated}
                onChange={(event) => setDraft({ ...draft, activated: event.target.checked })}
              />
              Activated
            </label>
            <div className="user-actions">
              <button
                type="button"
                className="ghost"
                disabled={busyUserId !== null}
                onClick={() => {
                  setEditingUser(null);
                  setDraft(null);
                }}
              >
                Cancel
              </button>
              <button type="submit" disabled={busyUserId !== null}>
                {busyUserId ? "Saving..." : "Save changes"}
              </button>
            </div>
          </form>
        </div>
      ) : null}

      {deletingUser ? (
        <div
          className="dialog-backdrop"
          onClick={() => {
            if (!busyUserId) setDeletingUser(null);
          }}
        >
          <div
            className="card dialog-card"
            role="dialog"
            aria-modal="true"
            aria-labelledby="delete-user-title"
            onClick={(event) => event.stopPropagation()}
          >
            <h2 id="delete-user-title">Delete user</h2>
            <p>
              Permanently delete {displayName(deletingUser)} ({deletingUser.email})? If this account still has related
              records, it will be deactivated instead. Super admins and your own account cannot be deleted here.
            </p>
            <div className="user-actions">
              <button type="button" className="ghost" disabled={busyUserId !== null} onClick={() => setDeletingUser(null)}>
                Cancel
              </button>
              <button type="button" className="danger" disabled={busyUserId !== null} onClick={() => void handleDelete()}>
                {busyUserId ? "Deleting..." : "Delete user"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </>
  );
}
