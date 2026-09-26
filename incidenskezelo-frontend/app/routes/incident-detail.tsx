import { Tabs, Text } from "@mantine/core";
import { IncidentDetailHeader } from "~/components/IncidentDetailHeader";
import { IncidentOverview } from "~/components/IncidentOverview";
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
    <>
      <IncidentDetailHeader incident={incident} />

      <Tabs key={incident.incidentId} defaultValue="overview" color="#0f717d" className="flex min-h-0 flex-1 flex-col">
        <Tabs.List className="mx-[22px] mt-[22px] max-[680px]:mx-4">
          <Tabs.Tab value="overview">Áttekintés</Tabs.Tab>
        </Tabs.List>

        <div className="min-h-0 flex-1 overflow-y-auto p-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
          <Tabs.Panel value="overview">
            <IncidentOverview incident={incident} />
          </Tabs.Panel>
        </div>
      </Tabs>
    </>
  );
}
