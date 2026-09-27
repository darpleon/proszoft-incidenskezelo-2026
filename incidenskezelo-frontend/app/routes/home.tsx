import { useEffect, useState } from "react";
import type { Route } from "./+types/home";
import api from "~/api/api";

type ConnectionStatus = "checking" | "connected" | "failed";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Incidenskezelő" },
    { name: "description", content: "Incidenskezelő alkalmazás" },
  ];
}

export default function Home() {
  const [status, setStatus] = useState<ConnectionStatus>("checking");

  useEffect(() => {
    api.Health.getHealth()
      .then(() => setStatus("connected"))
      .catch(() => setStatus("failed"));
  }, []);

  return (
    <main>
      <h1>Incidenskezelő</h1>
      <p>Backend kapcsolat: {status}</p>
    </main>
  );
}
