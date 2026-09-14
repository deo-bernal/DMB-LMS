import { Navigate } from "react-router-dom";
import { useAuth } from "../contexts/JWTAuthContext";

export function ProtectedRoute({ children, allowedRoles }: { children: JSX.Element; allowedRoles?: string[] }) {
  const { isAuthenticated, currentRole } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (allowedRoles && !allowedRoles.includes(currentRole)) {
    return <div className="card">You do not have access to this page.</div>;
  }
  return children;
}
