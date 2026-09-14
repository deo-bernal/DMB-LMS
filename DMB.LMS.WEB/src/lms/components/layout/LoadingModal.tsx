import { useEffect, useState } from "react";
import { subscribeLoading } from "../../utils/loadingGate";

export default function LoadingModal() {
  const [pending, setPending] = useState(0);
  const [visible, setVisible] = useState(false);

  useEffect(() => subscribeLoading(setPending), []);

  useEffect(() => {
    if (pending <= 0) {
      setVisible(false);
      return;
    }
    const timer = window.setTimeout(() => setVisible(true), 80);
    return () => window.clearTimeout(timer);
  }, [pending]);

  if (!visible) return null;

  return (
    <div className="loading-modal" role="dialog" aria-modal="true" aria-label="Loading">
      <div className="loading-modal-card">
        <span className="loading-modal-spinner" aria-hidden />
        <span>Loading</span>
      </div>
    </div>
  );
}
