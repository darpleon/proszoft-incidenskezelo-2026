import { SegmentedControl, Text, Title } from "@mantine/core";
import { useState } from "react";
import { IncidentListItem } from "./IncidentListItem";
import { currentUser } from "~/data/currentUser";
import type { IIncident } from "~/interfaces/IIncident";
import { isOpen } from "~/lib/labels";

type IncidentListProps = {
  incidents: IIncident[];
};

type OwnerFilter = "all" | "mine";

export function IncidentList({ incidents }: IncidentListProps) {
  const [ownerFilter, setOwnerFilter] = useState<OwnerFilter>("all");

  const visibleIncidents = incidents
    .filter((incident) => ownerFilter === "all" || incident.assigneeUserId === currentUser.userId)
    .sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
  const openCount = visibleIncidents.filter((incident) => isOpen(incident.status)).length;

  return (
    <section className="flex w-[312px] shrink-0 flex-col border-r border-list-line bg-list-bg max-[1280px]:w-[272px] max-[920px]:max-h-[42vh] max-[920px]:w-full max-[920px]:border-r-0 max-[920px]:border-b">
      <div className="flex items-center gap-[9px] px-4 pt-[13px] pb-[11px]">
        <Title order={2} className="text-sm font-semibold tracking-tight">
          Incidensek
        </Title>
        <Text size="xs" className="text-text-2">
          {openCount} nyitott
        </Text>
        <SegmentedControl
          ml="auto"
          size="xs"
          value={ownerFilter}
          onChange={(value) => setOwnerFilter(value as OwnerFilter)}
          data={[
            { value: "all", label: "Összes" },
            { value: "mine", label: "Enyém" },
          ]}
        />
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto pb-2">
        {visibleIncidents.map((incident) => (
          <IncidentListItem key={incident.incidentId} incident={incident} />
        ))}
        {visibleIncidents.length === 0 && (
          <Text size="sm" className="px-4 py-6 text-center text-text-3">
            Nincs megjeleníthető incidens.
          </Text>
        )}
      </div>
    </section>
  );
}
