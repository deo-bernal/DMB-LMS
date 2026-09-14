import { useEffect, useState } from "react";
import { subscribeLoading } from "../../utils/loadingGate";

export default function LoadingModal() {
  const [pending, setPending] = useState(0);
  useEffect(() => subscribeLoading(setPending), []);
  if (!pending) return null;
  return (
    <div className="loading-modal">
      <div className="loading-modal-card">Loading…</div>
    </div>
  );
}
