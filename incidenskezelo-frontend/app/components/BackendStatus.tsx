import { useEffect, useState } from "react";
import api from "~/api/api";

type ConnectionStatus = "checking" | "connected" | "failed";

const statusLabel: Record<ConnectionStatus, string> = {
  checking: "Kapcsolódás...",
  connected: "Backend elérhető",
  failed: "Backend nem elérhető",
};

const dotColor: Record<ConnectionStatus, string> = {
  checking: "bg-nav-dim",
  connected: "bg-ok",
  failed: "bg-crit",
};

export function BackendStatus() {
  const [status, setStatus] = useState<ConnectionStatus>("checking");

  useEffect(() => {
    api.Health.getHealth()
      .then((response) => setStatus(response.data.database === "ok" ? "connected" : "failed"))
      .catch(() => setStatus("failed"));
  }, []);

  return (
    <div className="flex items-center gap-2 text-xs text-nav-dim whitespace-nowrap">
      <i className={`size-1.5 rounded-full ${dotColor[status]}`} />
      {statusLabel[status]}
    </div>
  );
}
