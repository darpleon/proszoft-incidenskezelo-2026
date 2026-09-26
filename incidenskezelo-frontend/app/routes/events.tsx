import { Title } from "@mantine/core";
import type { Route } from "./+types/events";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Események | Incidenskezelő" }];
}

export default function Events() {
  return (
    <div className="p-6">
      <Title order={1} size="h3">
        Események
      </Title>
    </div>
  );
}
