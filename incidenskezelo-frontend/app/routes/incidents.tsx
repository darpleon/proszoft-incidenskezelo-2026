import { Title } from "@mantine/core";
import type { Route } from "./+types/incidents";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Incidensek | Incidenskezelő" }];
}

export default function Incidents() {
  return (
    <div className="p-6">
      <Title order={1} size="h3">
        Incidensek
      </Title>
    </div>
  );
}
