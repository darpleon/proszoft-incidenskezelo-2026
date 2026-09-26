import { Text, Title } from "@mantine/core";
import { incidents } from "~/data/incidents";
import type { Route } from "./+types/incident-detail";

export default function IncidentDetail({ params }: Route.ComponentProps) {
  const incident = incidents.find((item) => item.incidentId === Number(params.incidentId));

  if (!incident) {
    return (
      <Text size="sm" className="p-6 text-text-3">
        Az incidens nem található.
      </Text>
    );
  }

  return (
    <div className="px-[22px] pt-[22px] max-[680px]:px-4">
      <Text size="xs" fw={500} className="tracking-wider text-text-3">
        INC-{incident.incidentId}
      </Text>
      <Title order={1} className="mt-[3px] text-2xl font-semibold tracking-tight max-[680px]:text-xl">
        {incident.title}
      </Title>
    </div>
  );
}
