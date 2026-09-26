import { Tabs, Text } from "@mantine/core";
import { IncidentDetailHeader } from "~/components/IncidentDetailHeader";
import { EventsTable } from "~/components/EventsTable";
import { IncidentOverview } from "~/components/IncidentOverview";
import { events } from "~/data/events";
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

  const incidentEvents = events
    .filter((event) => event.incidentId === incident.incidentId)
    .sort((a, b) => a.occurredAtUtc.localeCompare(b.occurredAtUtc));

  return (
    <>
      <IncidentDetailHeader incident={incident} events={incidentEvents} />

      <Tabs key={incident.incidentId} defaultValue="overview" color="#0f717d" className="flex min-h-0 flex-1 flex-col">
        <Tabs.List className="mx-[22px] mt-[22px] max-[680px]:mx-4">
          <Tabs.Tab value="overview">Áttekintés</Tabs.Tab>
          <Tabs.Tab
            value="events"
            rightSection={
              <span className="grid h-[17px] min-w-[17px] place-items-center rounded-full bg-[#eff2f1] px-[5px] text-[10px] font-semibold text-text-2">
                {incidentEvents.length}
              </span>
            }
          >
            Események
          </Tabs.Tab>
        </Tabs.List>

        <div className="min-h-0 flex-1 overflow-y-auto p-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
          <Tabs.Panel value="overview">
            <IncidentOverview incident={incident} />
          </Tabs.Panel>
          <Tabs.Panel value="events">
            <EventsTable events={incidentEvents} />
          </Tabs.Panel>
        </div>
      </Tabs>
    </>
  );
}
